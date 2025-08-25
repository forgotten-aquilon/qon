using qon.Rules.Aggregators;
using qon.Rules.Filters;
using qon.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace qon.Rules
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

        public override ConstraintResult Execute(List<SuperpositionVariable<T>> field)
        {
            switch (AggregationType)
            {
                case AggregationType.Grouping:
                    var changes = 0;
                    var unsolvedChanges = 0;
                    var groups = field.GroupBy(x => GroupingAggregator!.ApplyTo(x));

                    foreach (var group in groups)
                    {
                        var result = Filter.ApplyTo(group.ToList());
                        changes += result.ChangesAmount;
                        switch (result.Outcome)
                        {
                            case PropagationOutcome.UnderConstrained:
                                unsolvedChanges++;
                                break;
                            case PropagationOutcome.Converged:
                                break;
                            case PropagationOutcome.Conflict:
                                return result;
                        }
                    }

                    return unsolvedChanges == 0
                        ? new ConstraintResult(PropagationOutcome.Converged, changes)
                        : new ConstraintResult(PropagationOutcome.UnderConstrained, changes);

                case AggregationType.Selecting:
                    var aggregation = field.Where(SelectingAggregator!.ApplyTo).ToList();

                    return Filter.ApplyTo(aggregation);
                default:
                    break;
            }

            throw new NotImplementedException();
        }
    }
}
