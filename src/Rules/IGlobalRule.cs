using System.Collections.Generic;
using qon.Rules.Filters;
using qon.Variables;

namespace qon.Rules
{
    public interface IGlobalRule<T>
    {
        public ConstraintResult Execute(SuperpositionVariable<T>[] field);
    }
}
