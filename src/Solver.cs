using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using qon.Rules;

namespace qon
{
    public class Solver<T> : IEnumerator<MachineState<T>>
    {
        internal sealed class Node
        {
            public List<SuperpositionVariable<T>> Field { get; set; } = new List<SuperpositionVariable<T>>();
        }

        private readonly QMachine<T> _machine;

        private readonly Stack<Node> _solutionStack;

        public MachineState<T> Current => _machine.State;

        object IEnumerator.Current => Current;

        public Solver(QMachine<T> machine) 
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
                    var result = _machine.ApplyConstraints();
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
                    var validation = _machine.ApplyConstraints();

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
                    break;
                case MachineStateType.Error:
                    break;
                default:
                    break;
            }

            return changes != 0;
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