using qon.Helpers;
using qon.Variables;
using System;
using static qon.Helpers.Helper;

namespace qon.Rules.Aggregators
{
    public class SelectingAggregator<T>
    {
        public static readonly SelectingAggregator<T> Empty = new SelectingAggregator<T>(o => false);

        public Func<SuperpositionVariable<T>, bool> AggregationFunction { get; protected set; }

        public SelectingAggregator(Func<SuperpositionVariable<T>, bool> aggregationFunction)
        {
            AggregationFunction = aggregationFunction;
        }

        public bool ApplyTo(SuperpositionVariable<T> variable)
        {
            return AggregationFunction(variable);
        }

        public static SelectingAggregator<T> operator |(SelectingAggregator<T> left, SelectingAggregator<T> right)
        {
            return left.Or(right);
        }

        public static SelectingAggregator<T> operator &(SelectingAggregator<T> left, SelectingAggregator<T> right)
        {
            return left.And(right);
        }

        public static SelectingAggregator<T> Create<TVariable>(Func<TVariable, bool> predicate) where TVariable : SuperpositionVariable<T>
        {
            return new SelectingAggregator<T>(PredicateBuilder.For<T,TVariable>(predicate));
        }
    }
}