using qon.Functions.Filters;
using qon.Functions.Propagators;
using qon.Machines;
using qon.Variables;

namespace qon.Functions.Constraints
{
    public abstract class ConstraintBase<T> : IPreparation<T>
    {
        protected readonly FilteringType FilteringType;

        protected Filter<T>? GroupingAggregator { get; set; }
        protected QPredicate<T>? SelectingAggregator { get; set; }

        public abstract Result Execute(Field<T> field, QMachine<T>? machine);

        protected ConstraintBase(Filter<T> grouping)
        {
            GroupingAggregator = grouping;
            FilteringType = FilteringType.Grouping;
        }

        protected ConstraintBase(QPredicate<T> selecting)
        {
            SelectingAggregator = selecting;
            FilteringType = FilteringType.Selecting;
        }
    }
}
