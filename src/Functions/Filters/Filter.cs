using System;
using qon.Variables;

namespace qon.Functions.Filters
{
    public class Filter<TQ> : Chain<QObject<TQ>, object> where TQ : notnull
    {
        public Func<QObject<TQ>, object> AggregationFunction { get; }

        public Filter(Func<QObject<TQ>, object> aggregationFunction)
        {
            AggregationFunction = aggregationFunction;
        }

        public override object ApplyTo(QObject<TQ> input)
        {
            return AggregationFunction(input);
        }

        public static IChain<QObject<TQ>, object> operator~(Filter<TQ> obj)
        {
            return obj;
        }
    }
}
