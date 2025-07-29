using qon.Rules.Aggregators;
using System.Collections.Generic;
using qon.Rules.Filters;

namespace qon.Rules
{
    public abstract class GlobalRuleBase<T> : IGlobalRule<T>
    {
        protected readonly AggregationType AggregationType;

        protected GroupingAggregator<T>? GroupingAggregator { get; set; }
        protected SelectingAggregator<T>? SelectingAggregator { get; set; }

        public abstract ConstraintResult Execute(List<SuperpositionVariable<T>> field);

        protected GlobalRuleBase(GroupingAggregator<T> grouping)
        {
            GroupingAggregator = grouping;
            AggregationType = AggregationType.Grouping;
        }

        protected GlobalRuleBase(SelectingAggregator<T> selecting)
        {
            SelectingAggregator = selecting;
            AggregationType = AggregationType.Selecting;
        }
    }
}
