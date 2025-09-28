using qon.Variables;
using System;
using System.Runtime.CompilerServices;

namespace qon.Rules.Guards
{
    public static class Guards
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Guard<T> Equals<T>(in T value)
        {
            var cached = value;
            return new Guard<T>(variable => variable.Value.CheckValue(cached));
        }
    }
}
