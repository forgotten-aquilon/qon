using System;
using System.Linq;
using qon.Constraints.Aggregators;
using qon.Constraints.Filters;
using qon.Variables;

namespace qon.Constraints
{
    public class GlobalRule<T> : GlobalRuleBase<T>
    {
        protected Filter<T> Filter { get; set; }

        public GlobalRule(GroupingAggregator<T> grouping,
            Filter<T> method) : base(grouping)
        {
            Filter = method;
        }

        public GlobalRule(SelectingAggregator<T> selecting,
            Filter<T> method) : base(selecting)
        {
            Filter = method;
        }

        public override ConstraintResult Execute(SuperpositionVariable<T>[] field)
        {
            switch (AggregationType)
            {
                case AggregationType.Grouping:
                    var changes = 0;
                    var unsolvedChanges = 0;
                    var groups = field.GroupBy(x => GroupingAggregator!.ApplyTo(x));

                    foreach (var group in groups)
                    {
                        var result = Filter.ApplyTo(group.ToArray());
                        changes += result.ChangesAmount;

                        if (!result.TryHandleOutcome(ref unsolvedChanges, out var conflictResult))
                        {
                            return conflictResult;
                        }
                    }

                    return unsolvedChanges == 0
                        ? new ConstraintResult(PropagationOutcome.Converged, changes)
                        : new ConstraintResult(PropagationOutcome.UnderConstrained, changes);

                case AggregationType.Selecting:
                    var aggregation = field.Where(SelectingAggregator!.ApplyTo).ToList();

                    return Filter.ApplyTo(aggregation);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
