using qon.Functions.Propagators;
using qon.Variables;

namespace qon.Functions.Constraints
{
    public interface IQConstraint<T>
    {
        public ConstraintResult Execute(SuperpositionVariable<T>[] field);
    }
}
