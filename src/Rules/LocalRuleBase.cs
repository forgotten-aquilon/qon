using qon.Rules.Guards;
using System.Collections.Generic;
using System.Linq;
using qon.Rules.Filters;
using qon.Variables;

namespace qon.Rules
{
    public abstract class LocalRuleBase<T> : ILocalRule<T>
    {
        public Guard<T>[] VariableGuards { get; set; }

        protected LocalRuleBase(Guard<T>[] guards)
        {
            VariableGuards = guards;
        }

        public bool CanApplyTo(SuperpositionVariable<T> variable)
        {
            return VariableGuards.All(guard => guard.ApplyTo(variable));
        }

        public abstract ConstraintResult Execute(SuperpositionVariable<T>[] field, SuperpositionVariable<T> variable);
    }
}
