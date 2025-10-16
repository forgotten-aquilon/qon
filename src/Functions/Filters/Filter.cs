using System;
using qon.Functions;
using qon.Functions.Operations;
using qon.Variables;

namespace qon.Functions.Filters
{
    public class Filter<T> : IChain<QVariable<T>, object>
    {
        public Func<QVariable<T>, object> AggregationFunction { get; }

        public Filter(Func<QVariable<T>, object> aggregationFunction)
        {
            AggregationFunction = aggregationFunction;
        }

        public object ApplyTo(QVariable<T> input)
        {
            return AggregationFunction(input);
        }

        public static IChain<QVariable<T>, object> operator~(Filter<T> obj)
        {
            return obj;
        }
    }
}
