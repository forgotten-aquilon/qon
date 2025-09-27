using qon.Rules.Aggregators;
using qon.Rules.Filters;
using qon.Rules.Guards;
using qon.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static qon.Helpers.Helper;

namespace qon.Rules
{
    public class LocalRule<T> : LocalRuleBase<T>
    {
        sealed class Me
        {
            private Me() { }
            private void Self() { Console.WriteLine("self"); }
        }

        

        public Func<SuperpositionVariable<T>, SelectingAggregator<T>> AggregationFactory { get; set; }

        protected Filter<T> Filter { get; set; }

        public LocalRule(
            List<Guard<T>> guards,
            Func<SuperpositionVariable<T>, SelectingAggregator<T>> factory, 
            Filter<T> filter) : base(guards)
        {
            AggregationFactory = factory;
            Filter = filter;
        }

        public override ConstraintResult Execute(List<SuperpositionVariable<T>> field, SuperpositionVariable<T> variable)
        {
            var aggregator = AggregationFactory(variable);

            return Filter.ApplyTo(field.Where(aggregator.ApplyTo));
        }

        public static Func<SuperpositionVariable<T>, SelectingAggregator<T>> Create<TVariable>(Func<TVariable, SelectingAggregator<T>> predicate) where TVariable : SuperpositionVariable<T>
        {
            return PredicateBuilder.For<T, TVariable>(predicate);
        }
    }
}
