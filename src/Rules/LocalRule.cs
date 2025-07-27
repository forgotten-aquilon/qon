using System;
using System.Collections.Generic;
using System.Linq;

namespace qon.Rules
{
    public class LocalRule<T> : LocalRuleBase<T>
    {
        public List<Func<SuperpositionVariable<T>, SelectingAggregator<T>>> AggregationFactories { get; set; }

        protected Filter<T> Filter { get; set; }

        public LocalRule(
            List<Guard<T>> guards, 
            List<Func<SuperpositionVariable<T>, SelectingAggregator<T>>> factories, 
            Filter<T> filter) : base(guards)
        {
            AggregationFactories = factories;
            Filter = filter;
        }

        public override ConstraintResult Execute(List<SuperpositionVariable<T>> field, SuperpositionVariable<T> variable)
        {
            IEnumerable<SuperpositionVariable<T>> list = new List<SuperpositionVariable<T>>();
            foreach (var factory in AggregationFactories)
            {
                var aggregator = factory(variable);
                var aggregation = field.Where(aggregator.ApplyTo);
                list = list.Concat(aggregation);
            }

            return Filter.ApplyTo(list.Distinct().ToList());
        }
    }
}
