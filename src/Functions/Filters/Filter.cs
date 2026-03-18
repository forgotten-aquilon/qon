using System;
using qon.Variables;

namespace qon.Functions.Filters
{
    public class Filter<TQ> : Chain<QVariable<TQ>, object> where TQ : notnull
    {
        public Func<QVariable<TQ>, object> AggregationFunction { get; }

        public Filter(Func<QVariable<TQ>, object> aggregationFunction)
        {
            AggregationFunction = aggregationFunction;
        }

        public override object ApplyTo(QVariable<TQ> input)
        {
            return AggregationFunction(input);
        }

        public static IChain<QVariable<TQ>, object> operator~(Filter<TQ> obj)
        {
            return obj;
        }
    }
}
