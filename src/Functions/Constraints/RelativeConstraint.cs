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
    public class RelativeConstraint<TQ> : IPreparation<TQ> where TQ : notnull
    {
        protected QPredicate<TQ> Guard { get; set; }
        protected Propagator<TQ> Propagator { get; set; }
        public Func<QVariable<TQ>, QPredicate<TQ>> AggregationFactory { get; set; }

        public RelativeConstraint(QPredicate<TQ> guard, Propagator<TQ> propagator, Func<QVariable<TQ>, QPredicate<TQ>> aggregationFactory)
        {
            Guard = guard;
            Propagator = propagator;
            AggregationFactory = aggregationFactory;
        }

        public virtual Result Execute(Field<TQ> field)
        {
            IEnumerable<QVariable<TQ>>? relativeVariables = field.Where(Guard.ApplyTo);

            HashSet<QVariable<TQ>> aggregation = new();

            foreach (QVariable<TQ> relativeVariable in relativeVariables)
            {
                aggregation.UnionWith(field.Where(AggregationFactory(relativeVariable).ApplyTo));
            }

            return Propagator.ApplyTo(aggregation);
        }

        public static Func<QVariable<TQ>, QPredicate<TQ>> WithLayer<TLayer>(Func<TLayer, QPredicate<TQ>> predicate) where TLayer : ILayer<TQ, QVariable<TQ>>
        {
            return variable => predicate((TLayer)variable.LayerManager.GetLayer<TLayer>());
        }
    }
}
