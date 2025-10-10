using System;
using System.Collections.Generic;
using System.Linq;
using qon.Variables;

namespace qon.Constraints.Filters
{
    public class Filter<T>
    {
        public Func<IEnumerable<SuperpositionVariable<T>>, ConstraintResult> FilterFunction { get; }

        public Filter(Func<IEnumerable<SuperpositionVariable<T>>, ConstraintResult> filterFunction)
        {
            FilterFunction = filterFunction;
        }

        public ConstraintResult ApplyTo(IEnumerable<SuperpositionVariable<T>> filteringList)
        {
            ConstraintResult result = FilterFunction(filteringList);

            return result.Outcome switch
            {
                PropagationOutcome.Conflict => result,
                _ => filteringList.Any(x => x.State == SuperpositionState.Uncertain && x.Domain.IsEmpty()) 
                    ? new ConstraintResult(PropagationOutcome.Conflict, result.ChangesAmount)
                    : result
            };
        }
    }
}