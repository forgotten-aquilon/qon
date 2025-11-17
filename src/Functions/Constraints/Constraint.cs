using System;
using System.Linq;
using qon.Exceptions;
using qon.Functions.Filters;
using qon.Functions.Propagators;
using qon.Machines;
using qon.Variables;

namespace qon.Functions.Constraints
{
    public class Constraint<TQ> : ConstraintBase<TQ> where TQ : notnull
    {
        protected Propagator<TQ> Propagator { get; set; }

        public Constraint(Filter<TQ> grouping,
            Propagator<TQ> method) : base(grouping)
        {
            Propagator = method;
        }

        public Constraint(QPredicate<TQ> selecting,
            Propagator<TQ> method) : base(selecting)
        {
            Propagator = method;
        }

        public override Result Execute(Field<TQ> field, QMachine<TQ>? machine)
        {
            switch (FilteringType)
            {
                case FilteringType.Grouping:
                    var changes = 0;
                    var groups = field.GroupBy(x => GroupingAggregator!.ApplyTo(x));

                    foreach (var group in groups)
                    {
                        var result = Propagator.ApplyTo(group.ToArray());
                        changes += result.ChangesAmount;

                        if (result.Failed)
                        {
                            return result;
                        }
                    }

                    return Result.Success(changes);

                case FilteringType.Selecting:
                    var aggregation = field.Where(SelectingAggregator!.ApplyTo).ToList();

                    return Propagator.ApplyTo(aggregation);
                default:
                    throw new NonExhaustiveExpressionException(typeof(FilteringType), FilteringType);
            }
        }
    }
}
