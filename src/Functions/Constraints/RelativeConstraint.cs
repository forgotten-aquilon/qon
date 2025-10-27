using qon.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Functions.Propagators;
using static qon.Helpers.Helper;
using qon.Functions.Filters;
using qon.Layers;
using qon.Machines;

namespace qon.Functions.Constraints
{
    public class RelativeConstraint<T> : IPreparation<T>
    {
        protected QPredicate<T> Guard { get; set; }
        protected Propagator<T> Propagator { get; set; }
        public Func<QVariable<T>, QPredicate<T>> AggregationFactory { get; set; }

        public RelativeConstraint(QPredicate<T> guard, Propagator<T> propagator, Func<QVariable<T>, QPredicate<T>> aggregationFactory)
        {
            Guard = guard;
            Propagator = propagator;
            AggregationFactory = aggregationFactory;
        }

        public virtual Result Execute(QVariable<T>[] field, QMachine<T>? machine)
        {
            IEnumerable<QVariable<T>>? relativeVariables = field.Where(Guard.ApplyTo);

            HashSet<QVariable<T>> aggregation = new();

            foreach (QVariable<T> relativeVariable in relativeVariables)
            {
                aggregation.UnionWith(field.Where(AggregationFactory(relativeVariable).ApplyTo));
            }

            return Propagator.ApplyTo(aggregation);
        }

        public static Func<QVariable<T>, QPredicate<T>> WithLayer<TLayer>(Func<TLayer, QPredicate<T>> predicate) where TLayer : ILayer<T, QVariable<T>>
        {
            return variable => predicate((TLayer)variable.Layers.GetLayer<TLayer>());
        }
    }
}
