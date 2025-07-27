using System;

namespace qon.Rules
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

    public static class Guards
    {
        public static Guard<T> Equals<T>(T value)
        {
            return new Guard<T>(variable => variable.Value.CheckValue(value));
        }
    }
}
