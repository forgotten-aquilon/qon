using qon.Exceptions;
using qon.Variables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using qon.Functions;
using qon.Helpers;
using qon.Layers.StateLayers;
using qon.Machines;

namespace qon.Solvers
{
    /// <summary>
    /// Default implementation of <see cref="ISolver{TQ}"/>
    /// </summary>
    /// <typeparam name="TQ"></typeparam>
    public class DefaultSolver<TQ> : ISolver<TQ> where TQ : notnull
    {
        public class SolverParameter
        {
            public bool BackTrackingEnabled { get; set; } = true;

            public Func<int, int>? BackTrackingStrategy { get; set; }
        }

        public SolverParameter Parameter { get; set; }

        /// <summary>
        /// Function used for initialization of Solver with the instance of <see cref="QMachine{TQ}"/>
        /// </summary>
        public static Func<QMachine<TQ>, ISolver<TQ>> Injection => (machine) => new DefaultSolver<TQ>(machine, new SolverParameter());

        /// <summary>
        /// Current amount of all steps performed by Solver
        /// </summary>
        public int StepCounter { get; protected set; } = -1;

        /// <summary>
        /// Current amount of all steps performed by Solver while backtracking
        /// </summary>
        public int BackStepCounter { get; protected set; } = -1;

        public bool BackTrackingEnabled { get; } = true;

        /// <summary>
        /// Instance of the current Machine
        /// </summary>
        public QMachine<TQ> Machine { get; }

        /// <summary>
        /// Instance of the current <see cref="MachineState{TQ}"/>
        /// </summary>
        public MachineState<TQ> Current => Machine.State;

        object IEnumerator.Current => Current;

        /// <summary>
        /// Stack discrete steps made by Solver, representing the state of the Field
        /// </summary>
        private readonly Stack<Field<TQ>> _solutionStack;

        private DefaultSolver(QMachine<TQ> machine, SolverParameter parameter)
        {
            _solutionStack = new Stack<Field<TQ>>();

            Machine = machine;

            Parameter = parameter;

            BackTrackingEnabled = Parameter.BackTrackingEnabled;
        }

        /// <summary>
        /// Adds <see cref="Current"/> Field to the <see cref="_solutionStack"/>
        /// </summary>
        /// <returns></returns>
        private int GoForth()
        {
            if (_solutionStack.Count == 0 || Parameter.BackTrackingEnabled)
            {
                _solutionStack.Push(Current.Field.Copy());
            }

            StepCounter++;
            return 1;
        }

        /// <summary>
        /// Removes the top element of <see cref="_solutionStack"/> , updates field of <see cref="Current"/> with the
        /// new top element from the stack, or sets status to Error
        /// </summary>
        /// <returns></returns>
        private int GoBack()
        {
            if (Parameter.BackTrackingStrategy is { } strategy)
            {
                for (int i = 0; i < strategy(_solutionStack.Count); i++)
                {
                    _solutionStack.Pop();
                }
            }
            else
            {
                _solutionStack.Pop();
            }

            if (_solutionStack.Count == 0)
            {
                Machine.Status = MachineStateType.Error;
                return 1;
            }

            var field = _solutionStack.Peek();

            Current.SetField(field.Copy().Variables);

            BackStepCounter++;

            Machine.Status = MachineStateType.IsSolving;
            return 1;
        }

        /// <summary>
        /// Main loop for step-by-step calculation of the Solution, based on specified rules.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InternalLogicException"></exception>
        public bool MoveNext()
        {
            ExceptionHelper.ThrowIfFieldIsNull(Machine, nameof(Machine));

            int changes = 0;

            switch (Machine.Status)
            {
                case MachineStateType.Created:
                    throw new InternalLogicException("Machine is not prepared for solving");
                case MachineStateType.Ready:
                    changes += GoForth();
                    Machine.Status = MachineStateType.IsSolving;
                    break;
                case MachineStateType.IsSolving:
                    var result = Prepare();
                    changes += result.ChangesAmount;

                    if (result.Failed)
                    {
                        changes += GoBack();
                        break;
                    }

                    var preValidationResult = PreValidate();

                    if (preValidationResult == PreValidationResult.PreValidated)
                    {
                        Machine.Status = MachineStateType.Validation;
                        changes++;
                    }
                    else if (preValidationResult == PreValidationResult.InvalidState)
                    {
                        changes += GoBack();
                    }
                    else if (preValidationResult == PreValidationResult.NotValidated)
                    {
                        Execute();

                        changes += GoForth();
                    }

                    break;
                case MachineStateType.Validation:
                    if (Validate())
                    {
                        changes += GoForth();
                        Machine.Status = MachineStateType.Finished;
                    }
                    else
                    {
                        changes += GoBack();
                    }

                    break;
                case MachineStateType.Finished:
                    return false;
                case MachineStateType.Error:
                    return false;
                default:
                    break;
            }

            return changes != 0;
        }

        /// <summary>
        /// Preparation stage in a calculation step. Sequentially calls all <see cref="IStateLayer{TQ}"/> layers to
        /// perform initial step in solving.
        /// </summary>
        /// <returns></returns>
        public Result Prepare()
        {
            int totalChanges = 0;

            foreach (var layer in Current.LayerManager.Layers)
            {
                if (layer is not IStateLayer<TQ> stateLayer) 
                    continue;

                var result = stateLayer.Prepare(Current.Field);

                if (result.Failed)
                {
                    return result;
                }

                totalChanges += result.ChangesAmount;
            }

            return Result.Success(totalChanges);
        }

        /// <summary>
        /// Prevalidation stage in a calculation step. Sequentially calls all <see cref="IStateLayer{TQ}"/> layers to
        /// perform main step in solving. On this step it's possible to determine, should the calculation be continued
        /// or returned to a previous step.
        /// </summary>
        /// <returns></returns>
        public PreValidationResult PreValidate()
        {
            PreValidationResult result = PreValidationResult.PreValidated;

            foreach (var layer in Current.LayerManager.Layers)
            {
                if (layer is not IStateLayer<TQ> stateLayer)
                    continue;

                var layerResult = stateLayer.PreValidate(Current.Field);

                if (layerResult == PreValidationResult.InvalidState)
                {
                    return PreValidationResult.InvalidState;
                }

                if (layerResult == PreValidationResult.NotValidated)
                {
                    result = PreValidationResult.NotValidated;
                }
            }

            return result;
        }

        /// <summary>
        /// Validation stage in a calculation step. Sequentially calls all <see cref="IStateLayer{TQ}"/> layers to check
        /// compliance of current solution with all rules.
        /// </summary>
        /// <returns></returns>
        public bool Validate()
        {
            foreach (var layer in Current.LayerManager.Layers)
            {
                if (layer is not IStateLayer<TQ> stateLayer) 
                    continue;

                if (stateLayer.Validate(Current.Field)) 
                    continue;
                    
                return false;
            }

            return true;
        }

        /// <summary>
        /// Execution stage in a calculation step Sequentially calls all <see cref="IStateLayer{TQ}"/> layers to update
        /// current field according to performed calculations.
        /// </summary>
        public void Execute()
        {
            foreach (var layer in Current.LayerManager.Layers)
            {
                if (layer is not IStateLayer<TQ> stateLayer)
                    continue;

                _solutionStack.TryPeek(out var previousField);

                stateLayer.Execute(previousField, Current.Field);
            }
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public static Func<QMachine<TQ>, ISolver<TQ>> InjectWith(SolverParameter parameter)
        {
            return (machine) => new DefaultSolver<TQ>(machine, parameter);
        }
    }
}
