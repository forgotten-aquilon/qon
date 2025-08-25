using qon.Variables;
using System;

namespace qon.Rules.Guards
{
    public class Guard<T>
    {
        public Func<SuperpositionVariable<T>, bool> Condition { get; }
        public Guard(Func<SuperpositionVariable<T>, bool> condition)
        {
            Condition = condition;
        }

        public bool ApplyTo(SuperpositionVariable<T> variable)
        {
            return Condition(variable);
        }
    }
}