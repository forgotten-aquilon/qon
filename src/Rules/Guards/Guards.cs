using qon.Variables;
using System;

namespace qon.Rules.Guards
{
    public static class Guards
    {
        public static Guard<T> Equals<T>(in T value)
        {
            var cached = value;
            return new Guard<T>(variable => variable.Value.CheckValue(cached));
        }
    }
}
