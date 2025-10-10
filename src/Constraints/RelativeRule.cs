using qon.Constraints.Aggregators;
using qon.Constraints.Filters;
using qon.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static qon.Helpers.Helper;

namespace qon.Constraints
{
    public class RelativeRule<T> : IGlobalRule<T>
    {
        protected SelectingAggregator<T> Guard { get; set; }
        protected Filter<T> Filter { get; set; }
        public Func<SuperpositionVariable<T>, SelectingAggregator<T>> AggregationFactory { get; set; }

        public RelativeRule(SelectingAggregator<T> guard, Filter<T> filter, Func<SuperpositionVariable<T>, SelectingAggregator<T>> aggregationFactory)
        {
            Guard = guard;
            Filter = filter;
            AggregationFactory = aggregationFactory;
        }

        public ConstraintResult Execute(SuperpositionVariable<T>[] field)
        {
            IEnumerable<SuperpositionVariable<T>>? relativeVariables = field.Where(Guard.ApplyTo);

            HashSet<SuperpositionVariable<T>> aggregation = new();

            foreach (SuperpositionVariable<T> relativeVariable in relativeVariables)
            {
                aggregation.UnionWith(field.Where(AggregationFactory(relativeVariable).ApplyTo));
            }

            return Filter.ApplyTo(aggregation);
        }

        public static Func<SuperpositionVariable<T>, SelectingAggregator<T>> Create<TVariable>(Func<TVariable, SelectingAggregator<T>> predicate) where TVariable : SuperpositionVariable<T>
        {
            return PredicateBuilder.For<T, TVariable>(predicate);
        }
    }
}
