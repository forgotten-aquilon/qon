using System;
using System.Linq;
using qon.Functions.Filters;
using qon.Functions.Propagators;
using qon.Machines;
using qon.Variables;

namespace qon.Functions.Constraints
{
    public class Constraint<T> : ConstraintBase<T>
    {
        protected Propagator<T> Propagator { get; set; }

        public Constraint(Filter<T> grouping,
            Propagator<T> method) : base(grouping)
        {
            Propagator = method;
        }

        public Constraint(QPredicate<T> selecting,
            Propagator<T> method) : base(selecting)
        {
            Propagator = method;
        }

        public override Result Execute(QVariable<T>[] field, QMachine<T>? machine)
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
                    throw new NotImplementedException();
            }
        }
    }
}
