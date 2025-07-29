using System.Collections.Generic;
using qon.Rules.Filters;

namespace qon.Rules
{
    public interface IGlobalRule<T>
    {
        public ConstraintResult Execute(List<SuperpositionVariable<T>> field);
    }
}
