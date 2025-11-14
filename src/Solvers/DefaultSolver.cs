using qon.Exceptions;
using qon.Variables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using qon.Functions;
using qon.Layers.StateLayers;
using qon.Machines;

namespace qon.Solvers
{
    public class DefaultSolver<T> : ISolver<T>
    {
        public int StepCounter { get; protected set; } = -1;
        public int BackStepCounter { get; protected set; } = -1;

        protected readonly QMachine<T> _machine;
        public MachineState<T> Current => _machine.State;

        object IEnumerator.Current => Current;
    
        private readonly Stack<Field<T>> _solutionStack;

        public DefaultSolver(QMachine<T> machine)
        {
            _solutionStack = new Stack<Field<T>>();
            _machine = machine;
        }

        private int GoForth()
        {
            _solutionStack.Push(Current.Field.Copy());
            StepCounter++;
            return 1;
        }

        private int GoBack()
        {
            _solutionStack.Pop();

            if (_solutionStack.Count == 0)
            {
                _machine.StateType = MachineStateType.Error;
                return 1;
            }

            Current.SetField(_solutionStack.Peek().Copy().Variables);

            BackStepCounter++;

            return 1;
        }

        public bool MoveNext()
        {
            ExceptionHelper.ThrowIfFieldIsNull(_machine, nameof(_machine));

            int changes = 0;

            switch (_machine.StateType)
            {
                case MachineStateType.Created:
                    throw new InternalLogicException("Machine is not prepared for solving");
                case MachineStateType.Ready:
                    changes += GoForth();
                    _machine.StateType = MachineStateType.IsSolving;
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
                        _machine.StateType = MachineStateType.Validation;
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
                        _machine.StateType = MachineStateType.Finished;
                    }
                    else
                    {
                        changes += GoBack();
                        _machine.StateType = MachineStateType.IsSolving;
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

        public Result Prepare()
        {
            int totalChanges = 0;

            foreach (var layer in Current.Layers.SortedByPriority())
            {
                if (layer is not IStateLayer<T> stateLayer) 
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

        public bool Validate()
        {
            foreach (var layer in Current.Layers.SortedByPriority())
            {
                if (layer is not IStateLayer<T> stateLayer) 
                    continue;

                if (stateLayer.Validate(Current.Field)) 
                    continue;
                    
                return false;
            }

            return true;
        }

        public PreValidationResult PreValidate()
        {
            PreValidationResult result = PreValidationResult.PreValidated;

            foreach (var layer in Current.Layers.SortedByPriority())
            {
                if (layer is not IStateLayer<T> stateLayer)
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

        public void Execute()
        {
            foreach (var layer in Current.Layers.SortedByPriority())
            {
                if (layer is not IStateLayer<T> stateLayer)
                    continue;

                _solutionStack.TryPeek(out var previousField);

                stateLayer.Execute(previousField, Current.Field, _machine.Random);
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
    }
}
