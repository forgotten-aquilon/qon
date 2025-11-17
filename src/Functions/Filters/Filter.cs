using System;
using qon.Functions;
using qon.Functions.Operations;
using qon.Variables;

namespace qon.Functions.Filters
{
    public class Filter<TQ> : IChain<QVariable<TQ>, object> where TQ : notnull
    {
        public Func<QVariable<TQ>, object> AggregationFunction { get; }

        public Filter(Func<QVariable<TQ>, object> aggregationFunction)
        {
            AggregationFunction = aggregationFunction;
        }

        public object ApplyTo(QVariable<TQ> input)
        {
            return AggregationFunction(input);
        }

        public static IChain<QVariable<TQ>, object> operator~(Filter<TQ> obj)
        {
            return obj;
        }
    }
}
