using System;
using System.Linq;
using qon.Functions.Filters;
using qon.Functions.Propagators;
using qon.Variables;

namespace qon.Functions.Constraints
{
    public class QConstraint<T> : QConstraintBase<T>
    {
        protected Propagator<T> Propagator { get; set; }

        public QConstraint(Filter<T> grouping,
            Propagator<T> method) : base(grouping)
        {
            Propagator = method;
        }

        public QConstraint(QPredicate<T> selecting,
            Propagator<T> method) : base(selecting)
        {
            Propagator = method;
        }

        public override ConstraintResult Execute(SuperpositionVariable<T>[] field)
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

                    return ConstraintResult.Success(changes);

                case FilteringType.Selecting:
                    var aggregation = field.Where(SelectingAggregator!.ApplyTo).ToList();

                    return Propagator.ApplyTo(aggregation);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
