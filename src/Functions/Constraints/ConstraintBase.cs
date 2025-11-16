using qon.Functions.Filters;
using qon.Functions.Propagators;
using qon.Machines;
using qon.Variables;

namespace qon.Functions.Constraints
{
    public abstract class ConstraintBase<TQ> : IPreparation<TQ> where TQ : notnull
    {
        protected readonly FilteringType FilteringType;

        protected Filter<TQ>? GroupingAggregator { get; set; }
        protected QPredicate<TQ>? SelectingAggregator { get; set; }

        public abstract Result Execute(Field<TQ> field, QMachine<TQ>? machine);

        protected ConstraintBase(Filter<TQ> grouping)
        {
            GroupingAggregator = grouping;
            FilteringType = FilteringType.Grouping;
        }

        protected ConstraintBase(QPredicate<TQ> selecting)
        {
            SelectingAggregator = selecting;
            FilteringType = FilteringType.Selecting;
        }
    }
}
