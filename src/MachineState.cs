using System;
using System.Collections.Generic;
using System.Linq;
using qon.Rules;

namespace qon
{
    public enum SolutionState
    {
        NotSolved,
        MaybeSolved,
        Unsolvable
    }

    /// <summary>
    /// "Hack" for searching for variables in the field by properties. Working without collection-expressions is not possible in Unity for now.
    /// I don't like recursive allocation, but by design solution machine is not supposed to run too often.
    /// TODO: Remove this as soon Unity supports recent .NET versions
    /// Or maybe keep, because public API looks much better this way.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SearchResult<T>
    {
        public IEnumerable<SuperpositionVariable<T>> Result { get; set; }

        public SearchResult(IEnumerable<SuperpositionVariable<T>> field)
        {
            Result = field;
        }

        public void Collapse(T value, bool isConstant = false)
        {
            foreach (var variable in Result)
            {
                variable.Collapse(value, isConstant);
            }
        }

        public SearchResult<T> this[string name, object value]
        {
            get
            {
                return new SearchResult<T>(Search(name, value));
            }
        }

        private IEnumerable<SuperpositionVariable<T>> Search(string name, object value)
        {
            return Result.Where(x => x.Properties[name].Item2.Equals(value));
        }
    }

    public class MachineState<T> : ICloneable
    {
        public List<SuperpositionVariable<T>> Field { get; set; }

        public SolutionState CurrentState
        {
            get
            {
                if (Field.Any(x => x.Domain.IsEmpty() && x.State == SuperpositionState.Uncertain))
                {
                    return SolutionState.Unsolvable;
                }

                if (!Field.Any(x => x.State == SuperpositionState.Uncertain))
                {
                    return SolutionState.MaybeSolved;
                }

                return SolutionState.NotSolved;
            }
        }

        public MachineState()
        {
            Field = new List<SuperpositionVariable<T>>();
        }

        public MachineState(List<SuperpositionVariable<T>> field)
        {
            Field = field;
        }

        public ConstraintResult ExecuteGlobalRules(List<IGlobalRule<T>> rules)
        {
            int changes = 0;
            int unsolvedChanges = 0;
            foreach (var rule in rules)
            {
                var result = rule.Execute(Field);

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
                ? new ConstraintResult { Outcome = PropagationOutcome.Converged, ChangesAmount = changes }
                : new ConstraintResult { Outcome = PropagationOutcome.UnderConstrained, ChangesAmount = changes };
        }

        public ConstraintResult ExecuteLocalRules(List<ILocalRule<T>> rules)
        {
            int changes = 0;
            int unsolvedChanges = 0;

            foreach (var variable in Field)
            {
                foreach (var rule in rules)
                {
                    if (!rule.CanApplyTo(variable)) 
                        continue;

                    var result = rule.Execute(Field, variable);

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
                ? new ConstraintResult { Outcome = PropagationOutcome.Converged, ChangesAmount = changes }
                : new ConstraintResult { Outcome = PropagationOutcome.UnderConstrained, ChangesAmount = changes };
        }

        public int AutoCollapse()
        {
            int changes = 0;

            foreach (var v in Field)
            {
                if (v.State != SuperpositionState.Uncertain) 
                    continue;

                if (v.AutoCollapse().HasValue)
                {
                    changes++;
                }
            }

            return changes;
        }

        public object Clone()
        {
            var clone = new MachineState<T>(Field.Select(x => x.Copy()).ToList());
            return clone;
        }

        public T this[List<(string Name, object Value)> filteringProperties, bool isConstant]
        {
            set
            {
                IEnumerable<SuperpositionVariable<T>> filteringList = SearchByProperties(filteringProperties);

                foreach (var variable in filteringList)
                {
                    variable.Collapse(value, isConstant);
                }
            }
        }

        public List<SuperpositionVariable<T>> this[List<(string Name, object Value)> filteringProperties]
        {
            get
            {
                IEnumerable<SuperpositionVariable<T>> filteringList = SearchByProperties(filteringProperties);

                return filteringList.ToList();
            }
        }

        public SearchResult<T> this[string name, object value]
        {
            get
            {
                var result = Field.Where(x => x.Properties[name].Item2.Equals(value));
                return new SearchResult<T>(result);
            }
        }

        public override string ToString()
        {
            string result = "{ ";

            foreach (var variable in Field)
            {
                result += variable.State != SuperpositionState.Uncertain ? $"{variable.Name}:[{variable.Value}] " : $"{variable.Name}:[{variable.Domain}] ";
            }

            result += "}";

            return result;
        }

        private IEnumerable<SuperpositionVariable<T>> SearchByProperties(List<(string, object)> filteringProperties)
        {
            IEnumerable<SuperpositionVariable<T>> filteringList = Field;

            foreach (var (name, value) in filteringProperties)
            {
                filteringList = filteringList.Where(x => x.Properties[name].Item2.Equals(value));
            }

            return filteringList;
        }
    }
}
