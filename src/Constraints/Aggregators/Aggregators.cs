using System.Runtime.CompilerServices;
using qon.Exceptions;
using qon.Variables;

namespace qon.Constraints.Aggregators
{
    public static class Aggregators
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GroupingAggregator<T> GroupByTag<T>(string s)
        {
            return new GroupingAggregator<T>(v => v.GetNullOrValueProperty(s) 
                ?? throw new InternalLogicException($"Variable '{v.Name}' is missing required tag '{s}' for grouping."));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SelectingAggregator<T> SelectByTagValue<T>(string s, object value)
        {
            return new SelectingAggregator<T>(v => object.Equals(v.GetNullOrValueProperty(s), value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SelectingAggregator<T> All<T>()
        {
            return new SelectingAggregator<T>(v => true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SelectingAggregator<T> Unassigned<T>()
        {
            return new SelectingAggregator<T>(v => v.State == SuperpositionState.Uncertain);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SelectingAggregator<T> Assigned<T>()
        {
            return new SelectingAggregator<T>(v => v.State != SuperpositionState.Uncertain);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SelectingAggregator<T> DomainContains<T>(T value)
        {
            return new SelectingAggregator<T>(v => v.Domain.ContainsValue(value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SelectingAggregator<T> EqualsToValue<T>(T value)
        {
            return new SelectingAggregator<T>(v =>
                v.State != SuperpositionState.Uncertain && v.Value.CheckValue(value));
        }
    }
}
