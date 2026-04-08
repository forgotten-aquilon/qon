using qon.Exceptions;
using qon.Functions.Filters;
using qon.Functions.Propagators;
using qon.Machines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace qon.Functions.Constraints
{
    public class GroupingConstraint<TQ> : IPreparation<TQ> where TQ : notnull
    {
        private readonly Filter<TQ> _groupingAggregator;
        private readonly Propagator<TQ> _propagator;

        public GroupingConstraint(Filter<TQ> groupingAggregator, Propagator<TQ> propagator)
        {
            _groupingAggregator = groupingAggregator;
            _propagator = propagator;
        }

        public Result Execute(Field<TQ> field)
        {
            var changes = 0;

            var groups = field.GroupBy(x => _groupingAggregator.ApplyTo(x));

            foreach (var group in groups)
            {
                var result = _propagator.ApplyTo(group.ToArray());
                changes += result.ChangesAmount;

                if (result.Failed)
                {
                    return result;
                }
            }

            return Result.Success(changes);
        }
    }
}
