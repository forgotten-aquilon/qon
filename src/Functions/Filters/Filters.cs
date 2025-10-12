using System.Runtime.CompilerServices;
using qon.Exceptions;
using qon.Variables;

namespace qon.Functions.Filters
{
    public static class Filters
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Filter<T> GroupByTag<T>(string s)
        {
            return new Filter<T>(v => v.GetNullOrValueProperty(s) 
                ?? throw new InternalLogicException($"Variable '{v.Name}' is missing required tag '{s}' for grouping."));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static QPredicate<T> SelectByTagValue<T>(string s, object value)
        {
            return new QPredicate<T>(v => object.Equals(v.GetNullOrValueProperty(s), value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static QPredicate<T> All<T>()
        {
            return new QPredicate<T>(v => true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static QPredicate<T> Unassigned<T>()
        {
            return new QPredicate<T>(v => v.State == SuperpositionState.Uncertain);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static QPredicate<T> Assigned<T>()
        {
            return new QPredicate<T>(v => v.State != SuperpositionState.Uncertain);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static QPredicate<T> DomainContains<T>(T value)
        {
            return new QPredicate<T>(v => v.Domain.ContainsValue(value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static QPredicate<T> EqualsToValue<T>(T value)
        {
            return new QPredicate<T>(v =>
                v.State != SuperpositionState.Uncertain && v.Value.CheckValue(value));
        }
    }
}
