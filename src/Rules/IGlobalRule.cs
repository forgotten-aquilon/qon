using System.Collections.Generic;

namespace qon.Rules
{
    public enum AggregationType
    {
        None,
        Grouping,
        Selecting
    }

    public enum PropagationOutcome
    {
        UnderConstrained,
        Converged,
        Conflict
    }

    public interface IGlobalRule<T>
    {
        public ConstraintResult Execute(List<SuperpositionVariable<T>> field);
    }
}
