using qon.Exceptions;
using qon.Variables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using qon.Functions.Propagators;
using qon.Functions.Constraints;
using qon.Layers.VariableLayers;

namespace qon.Solvers
{
    public class FiniteSolver<T> : BaseSolver<T>
    {
        private readonly Stack<QVariable<T>[]> _solutionStack;

        public FiniteSolver(QMachine<T> machine) : base(machine)
        {
            _solutionStack = new Stack<QVariable<T>[]>();
        }

        public int PushStack((string name, T value)? usedValue = null)
        {
            if (_solutionStack.TryPeek(out var field) && usedValue is (string, T) v)
            {
                SuperpositionLayer<T>.With(field[_machine.Indexer[v.name]]).Domain.Remove(v.value);
            }

            _solutionStack.Push(Current.Field.Select(x => x.Copy()).ToArray());

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

            _machine.State.SetField(_solutionStack.Peek().Select(x => x.Copy()).ToArray());

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
                case MachineStateType.Prepared:
                    changes += PushStack();
                    _machine.StateType = MachineStateType.IsSolving;
                    break;
                case MachineStateType.IsSolving:
                    var result = ApplyConstraints();
                    changes += result.ChangesAmount;

                    if (Current.CurrentState == SolutionState.MaybeSolved)
                    {
                        _machine.StateType = MachineStateType.Validation;
                        changes++;
                    }
                    else if (Current.CurrentState == SolutionState.Unsolvable)
                    {
                        changes += GoBack();
                    }
                    else if (Current.CurrentState == SolutionState.NotSolved)
                    {
                        if (result.Failed)
                        {
                            changes += GoBack();
                        }
                        else
                        {
                            double entropy = double.MaxValue;
                            QVariable<T>? candidate = null;

                            foreach (var item in Current.Field)
                            {
                                if (SuperpositionLayer<T>.With(item).State == SuperpositionState.Uncertain && 
                                    SuperpositionLayer<T>.With(item).Entropy is var newEntropy && 
                                    newEntropy < entropy)
                                {
                                    entropy = newEntropy;
                                    candidate = item;
                                }
                            }
                                

                            /*
                            var variablesByEntropy =
                                Current.Field
                                    .Where(z => SuperpositionLayer<T>.GetState(z) == SuperpositionState.Uncertain)
                                    .GroupBy(x => SuperpositionLayer<T>.GetEntropy(x))
                                    .OrderBy(g => g.Key)
                                    .FirstOrDefault();
                            
                            //TODO: Update this to use MinBy when it will be available in .NET 6+

                            //TODO: Add functionality to change selection of variables with equal entropy, e.g. random or by some algorithm 
                            //var minimalEntropyVariable = minimalEntropyVariables?.MinBy(_ => _machine.Random.Next());
                            //var minimalEntropyVariable = variablesByEntropy?.FirstOrDefault();
                            var variablesByEntropyCollection = variablesByEntropy as ICollection<QVariable<T>>;

                            var minimalEntropyVariable = variablesByEntropyCollection!.RandomItem(_machine.Random);
                            */
                            ExceptionHelper.ThrowIfInternalValueIsNull(candidate, nameof(candidate));

                            var newValue = SuperpositionLayer<T>.With(candidate).Domain.GetRandomValue(_machine.Random);

                            SuperpositionLayer<T>.Collapse(candidate, newValue);
                            changes += PushStack((candidate.Name, newValue));
                        }
                    }

                    break;
                case MachineStateType.Validation:
                    var validation = ApplyConstraints(_machine.Constraints.ValidationConstraints is not null);

                    if (validation.Failed)
                    {
                        changes += GoBack();
                        _machine.StateType = MachineStateType.IsSolving;
                    }
                    else
                    {
                        changes += PushStack();
                        _machine.StateType = MachineStateType.Finished;
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

        public ConstraintResult ApplyConstraints(bool validation = false)
        {
            int filterChanges = 0;
            bool isConverged = true;

            int changes;
            do
            {
                changes = 0;

                var globalResult = ExecuteGlobalRules(_machine.Constraints.GeneralConstraints);

                if (globalResult.Failed)
                {
                    return globalResult;
                }

                changes += globalResult.ChangesAmount;
                changes += Current.AutoCollapse();

                filterChanges += changes;
            }
            while (changes != 0);

            if (!validation)
            {
                return ConstraintResult.Success(filterChanges);
            }

            var globalValidation = ExecuteGlobalRules(_machine.Constraints.ValidationConstraints!);

            if (globalValidation.Failed)
            {
                return globalValidation;
            }

            int validationChanges = globalValidation.ChangesAmount;

            return ConstraintResult.Success(validationChanges);
        }

        public ConstraintResult ExecuteGlobalRules(List<IQConstraint<T>> rules)
        {
            int changes = 0;
            foreach (var rule in rules)
            {
                var result = rule.Execute(Current.Field);

                if (result.Failed)
                {
                    return result;
                }

                changes += result.ChangesAmount;
            }

            return ConstraintResult.Success(changes);
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
