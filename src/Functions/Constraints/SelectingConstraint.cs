using qon.Functions.Filters;
using qon.Functions.Propagators;
using qon.Machines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace qon.Functions.Constraints
{
    public class SelectingConstraint<TQ> : IPreparation<TQ> where TQ : notnull
    {
        private readonly QPredicate<TQ> _selectingAggregator;
        private readonly Propagator<TQ> _propagator;

        public SelectingConstraint(QPredicate<TQ> groupingAggregator, Propagator<TQ> propagator)
        {
            _selectingAggregator = groupingAggregator;
            _propagator = propagator;
        }

        public Result Execute(Field<TQ> field)
        {
            var aggregation = field.Where(_selectingAggregator.ApplyTo).ToList();

            return _propagator.ApplyTo(aggregation);
        }
    }
}
