using qon.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Functions.Propagators;
using qon.Functions.Filters;
using qon.Layers;
using qon.Machines;

namespace qon.Functions.Constraints
{
    public class RelativeConstraint<TQ> : IPreparation<TQ> where TQ : notnull
    {
        protected QPredicate<TQ> Guard { get; set; }
        protected Propagator<TQ> Propagator { get; set; }
        public Func<QObject<TQ>, QPredicate<TQ>> AggregationFactory { get; set; }

        public RelativeConstraint(QPredicate<TQ> guard, Propagator<TQ> propagator, Func<QObject<TQ>, QPredicate<TQ>> aggregationFactory)
        {
            Guard = guard;
            Propagator = propagator;
            AggregationFactory = aggregationFactory;
        }

        public virtual Result Execute(Field<TQ> field)
        {
            IEnumerable<QObject<TQ>> anchoredVariables = field.Where(Guard.ApplyTo);

            HashSet<QObject<TQ>> aggregation = new();

            foreach (QObject<TQ> anchor in anchoredVariables)
            {
                aggregation.UnionWith(field.Where(AggregationFactory(anchor).ApplyTo));
            }

            return Propagator.ApplyTo(aggregation);
        }

        public static Func<QObject<TQ>, QPredicate<TQ>> WithLayer<TLayer>(Func<TLayer, QPredicate<TQ>> predicate) where TLayer : ILayer<TQ, QObject<TQ>>
        {
            return variable => predicate((TLayer)variable.LayerManager.GetLayer<TLayer>());
        }
    }
}
