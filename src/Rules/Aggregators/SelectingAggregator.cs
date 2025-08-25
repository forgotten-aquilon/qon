using qon.Variables;
using System;

namespace qon.Rules.Aggregators
{
    public class SelectingAggregator<T>
    {
        public Func<SuperpositionVariable<T>, bool> AggregationFunction { get; }

        public SelectingAggregator(Func<SuperpositionVariable<T>, bool> aggregationFunction)
        {
            AggregationFunction = aggregationFunction;
        }

        public bool ApplyTo(SuperpositionVariable<T> variable)
        {
            return AggregationFunction(variable);
        }
    }
}