using qon.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Functions.Propagators;
using static qon.Helpers.Helper;
using qon.Functions.Filters;

namespace qon.Functions.Constraints
{
    public class RelativeConstraint<T> : IQConstraint<T>
    {
        protected QPredicate<T> Guard { get; set; }
        protected Propagator<T> Propagator { get; set; }
        public Func<SuperpositionVariable<T>, QPredicate<T>> AggregationFactory { get; set; }

        public RelativeConstraint(QPredicate<T> guard, Propagator<T> propagator, Func<SuperpositionVariable<T>, QPredicate<T>> aggregationFactory)
        {
            Guard = guard;
            Propagator = propagator;
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

            return Propagator.ApplyTo(aggregation);
        }

        public static Func<SuperpositionVariable<T>, QPredicate<T>> Create<TVariable>(Func<TVariable, QPredicate<T>> predicate) where TVariable : SuperpositionVariable<T>
        {
            return PredicateBuilder.For<T, TVariable>(predicate);
        }
    }
}
