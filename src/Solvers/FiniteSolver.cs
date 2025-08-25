using qon.Rules;
using qon.Rules.Filters;
using qon.Variables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace qon.Solvers
{
    public class FiniteSolver<T> : IEnumerator<MachineState<T>>
    {
        internal sealed class Node
        {
            public List<SuperpositionVariable<T>> Field { get; set; } = new List<SuperpositionVariable<T>>();
        }

        private readonly QMachine<T> _machine;

        private readonly Stack<Node> _solutionStack;

        public MachineState<T> Current => _machine.State;

        object IEnumerator.Current => Current;

        public FiniteSolver(QMachine<T> machine) 
        {
            _machine = machine;
            _solutionStack = new Stack<Node>();
        }

        public int PushStack((string name, T value)? usedValue = null)
        {
            if (_solutionStack.TryPeek(out var node) && usedValue is not null)
            {
                node.Field.FirstOrDefault(x => x.Name == usedValue?.name)?.RemoveFromDomain(new List<T>(){ usedValue.Value.value });
            }

            _solutionStack.Push(new Node { Field = Current.Field.Select(x => x.Copy()).ToList() });

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

            _machine.SetState(new MachineState<T>(_solutionStack.Peek().Field.Select(x => x.Copy()).ToList()));

            return 1;
        }

        public bool MoveNext()
        {
            if (_machine == null)
            {
                throw new FieldNullException(nameof(_machine));
            }

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
                        if (result.Outcome == PropagationOutcome.Conflict)
                        {
                            changes += GoBack();
                        }
                        else
                        {
                            var variablesByEntropy =
                                Current.Field
                                    .Where(z => z.State == SuperpositionState.Uncertain)
                                    .GroupBy(x => x.Entropy);

                            //TODO: Update this to use MinBy when it will be available in .NET 6+
                            var minimalEntropyVariables = variablesByEntropy
                                    .FirstOrDefault(v => v.Key == variablesByEntropy
                                    .Min(g => g.Key));

                            //var minimalEntropyVariable = minimalEntropyVariables?.MinBy(_ => _machine.Random.Next());

                            var minimalEntropyVariable = minimalEntropyVariables?.FirstOrDefault();

                            if (minimalEntropyVariable is null)
                            {       
                                throw new InternalNullException(nameof(minimalEntropyVariable));
                            }

                            var newValue = minimalEntropyVariable.Domain.GetRandomValue(_machine.Random);

                            minimalEntropyVariable.Collapse(newValue);
                            changes += PushStack((minimalEntropyVariable.Name, newValue));
                        }
                    }

                    break;
                case MachineStateType.Validation:
                    var validation = ApplyConstraints(_machine.ValidationRules is not null);

                    if (validation.Outcome == PropagationOutcome.Conflict)
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

                var globalResult = ExecuteGlobalRules(_machine.GeneralRules.GlobalRules);

                if (globalResult.Outcome == PropagationOutcome.Conflict)
                {
                    return globalResult;
                }

                if (globalResult.Outcome == PropagationOutcome.UnderConstrained)
                {
                    isConverged = false;
                }

                changes += globalResult.ChangesAmount;
                changes += Current.AutoCollapse();

                var localResult = ExecuteLocalRules(_machine.GeneralRules.LocalRules);

                if (localResult.Outcome == PropagationOutcome.Conflict)
                {
                    return localResult;
                }

                if (localResult.Outcome == PropagationOutcome.UnderConstrained)
                {
                    isConverged = false;
                }

                changes += localResult.ChangesAmount;
                changes += Current.AutoCollapse();

                filterChanges += changes;
            }
            while (changes != 0);

            if (!validation)
            {
                return isConverged switch
                {
                    true => new ConstraintResult(PropagationOutcome.Converged, filterChanges),
                    false => new ConstraintResult(PropagationOutcome.UnderConstrained, filterChanges)
                };
            }

            var globalValidation = ExecuteGlobalRules(_machine.ValidationRules!.GlobalRules);

            if (globalValidation.Outcome == PropagationOutcome.Conflict)
            {
                return globalValidation;
            }

            var localValidation = ExecuteLocalRules(_machine.ValidationRules!.LocalRules);

            if (localValidation.Outcome == PropagationOutcome.Conflict)
            {
                return localValidation;
            }

            int validationChanges = globalValidation.ChangesAmount + localValidation.ChangesAmount;

            return new ConstraintResult(PropagationOutcome.Converged, validationChanges);
        }

        public ConstraintResult ExecuteGlobalRules(List<IGlobalRule<T>> rules)
        {
            int changes = 0;
            int unsolvedChanges = 0;
            foreach (var rule in rules)
            {
                var result = rule.Execute(Current.Field);

                switch (result.Outcome)
                {
                    case PropagationOutcome.UnderConstrained:
                        unsolvedChanges++;
                        break;
                    case PropagationOutcome.Converged:
                        break;
                    case PropagationOutcome.Conflict:
                        return result;
                }

                changes += result.ChangesAmount;
            }

            return unsolvedChanges == 0
                ? new ConstraintResult(PropagationOutcome.Converged, changes)
                : new ConstraintResult(PropagationOutcome.UnderConstrained, changes);
        }

        public ConstraintResult ExecuteLocalRules(List<ILocalRule<T>> rules)
        {
            int changes = 0;
            int unsolvedChanges = 0;

            foreach (var variable in Current.Field)
            {
                foreach (var rule in rules)
                {
                    if (!rule.CanApplyTo(variable))
                        continue;

                    var result = rule.Execute(Current.Field, variable);

                    switch (result.Outcome)
                    {
                        case PropagationOutcome.UnderConstrained:
                            unsolvedChanges++;
                            break;
                        case PropagationOutcome.Converged:
                            break;
                        case PropagationOutcome.Conflict:
                            return result;
                        default:
                            throw new NotImplementedException();
                    }

                    changes += result.ChangesAmount;
                }
            }

            return unsolvedChanges == 0
                ? new ConstraintResult(PropagationOutcome.Converged, changes)
                : new ConstraintResult(PropagationOutcome.UnderConstrained, changes);
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