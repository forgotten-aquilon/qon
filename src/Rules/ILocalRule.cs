using System.Collections.Generic;

namespace qon.Rules
{
    public interface ILocalRule<T>
    {
        public bool CanApplyTo(SuperpositionVariable<T> variable);
        public ConstraintResult Execute(List<SuperpositionVariable<T>> field, SuperpositionVariable<T> variable);
    }
}
