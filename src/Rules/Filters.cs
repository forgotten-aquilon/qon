using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace qon.Rules
{
    public class Filter<T>
    {
        public Func<List<SuperpositionVariable<T>>, ConstraintResult> FilterFunction { get; }

        public Filter(Func<List<SuperpositionVariable<T>>, ConstraintResult> filterFunction)
        {
            FilterFunction = filterFunction;
        }

        public ConstraintResult ApplyTo(List<SuperpositionVariable<T>> filteringList)
        {
            return FilterFunction(filteringList);
        }
    }

    public static class Filters
    {
        public static ConstraintResult AllDistinctFilter<T>(List<SuperpositionVariable<T>> filteringList)
        {
            int changes = 0;

            var decided = filteringList.Where(x => x.State != SuperpositionState.Uncertain).Select(y => y.Value.Value);

            var certainVariablesCount = decided.Count();
            var distinctVariables = decided.Distinct();

            if (certainVariablesCount != distinctVariables.Count())
            {
                return new ConstraintResult { Outcome = PropagationOutcome.Conflict, ChangesAmount = 0 };
            }

            var openVariables = filteringList.Where(x => x.State == SuperpositionState.Uncertain);
            
            foreach (var variable in openVariables)
            {
                changes += variable.RemoveFromDomain(distinctVariables);
                variable.AutoCollapse();
            }
            

            return (!filteringList.Any(x => x.State == SuperpositionState.Uncertain)) switch
            {
                true => new ConstraintResult { Outcome = PropagationOutcome.Converged, ChangesAmount = changes },
                false => new ConstraintResult { Outcome = PropagationOutcome.UnderConstrained, ChangesAmount = changes }
            };
        }

        public static Filter<T> AllDistinct<T>()
        {
            return new Filter<T>(AllDistinctFilter);
        }

        public static Filter<T> Intersection<T>(IEnumerable<T> filteringCollection)
        {
            return new Filter<T>(list =>
            {
                int changes = 0;
                
                foreach (var variable in list)
                {
#pragma warning disable CS8714
                    Dictionary<T, int> intersection = new();
#pragma warning restore CS8714

                    foreach (var value in filteringCollection)
                    {
                        if (variable.Domain.TryGetWeight(value, out int weight))
                        {
                            intersection[value] = weight;
                        }
                    }

                    variable.Domain.Set(intersection);
                    

                    if (variable.Domain.IsEmpty())
                    {
                        return new ConstraintResult { Outcome = PropagationOutcome.Conflict, ChangesAmount = 0 };
                    }

                    if (variable.AutoCollapse() != Optional<T>.Empty)
                    {
                        changes++;
                    }
                }

                return (list.All(x => x.State != SuperpositionState.Uncertain)) switch
                {
                    true => new ConstraintResult { Outcome = PropagationOutcome.Converged, ChangesAmount = changes },
                    false => new ConstraintResult
                        { Outcome = PropagationOutcome.UnderConstrained, ChangesAmount = changes }
                };
            });
        }
    }
}
