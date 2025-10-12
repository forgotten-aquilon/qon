using qon.Functions.Filters;
using qon.Functions.Propagators;
using qon.Variables;

namespace qon.Functions.Constraints
{
    public abstract class QConstraintBase<T> : IQConstraint<T>
    {
        protected readonly FilteringType FilteringType;

        protected Filter<T>? GroupingAggregator { get; set; }
        protected QPredicate<T>? SelectingAggregator { get; set; }

        public abstract ConstraintResult Execute(SuperpositionVariable<T>[] field);

        protected QConstraintBase(Filter<T> grouping)
        {
            GroupingAggregator = grouping;
            FilteringType = FilteringType.Grouping;
        }

        protected QConstraintBase(QPredicate<T> selecting)
        {
            SelectingAggregator = selecting;
            FilteringType = FilteringType.Selecting;
        }
    }
}
