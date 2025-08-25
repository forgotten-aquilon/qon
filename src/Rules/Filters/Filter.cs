using qon.Variables;
using System;
using System.Collections.Generic;

namespace qon.Rules.Filters
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
}