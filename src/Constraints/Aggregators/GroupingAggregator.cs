using System;
using qon.Variables;

namespace qon.Constraints.Aggregators
{
    public class GroupingAggregator<T>
    {
        public Func<SuperpositionVariable<T>, object> AggregationFunction { get; }

        public GroupingAggregator(Func<SuperpositionVariable<T>, object> aggregationFunction)
        {
            AggregationFunction = aggregationFunction;
        }

        public object ApplyTo(SuperpositionVariable<T> variable)
        {
            return AggregationFunction(variable);
        }
    }
}