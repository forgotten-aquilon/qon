using System;
using qon.Functions;
using qon.Variables;

namespace qon.Functions.Filters
{
    public class Filter<T> : IChain<SuperpositionVariable<T>, object>
    {
        public Func<SuperpositionVariable<T>, object> AggregationFunction { get; }

        public Filter(Func<SuperpositionVariable<T>, object> aggregationFunction)
        {
            AggregationFunction = aggregationFunction;
        }

        public object ApplyTo(SuperpositionVariable<T> input)
        {
            return AggregationFunction(input);
        }

        public IChain<SuperpositionVariable<T>, object> AsIChain()
        {
            return this;
        }
    }
}