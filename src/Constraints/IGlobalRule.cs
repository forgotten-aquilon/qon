using qon.Constraints.Filters;
using qon.Variables;

namespace qon.Constraints
{
    public interface IGlobalRule<T>
    {
        public ConstraintResult Execute(SuperpositionVariable<T>[] field);
    }
}
