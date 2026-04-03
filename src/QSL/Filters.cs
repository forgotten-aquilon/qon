using qon.Functions;
using qon.Functions.Filters;
using qon.Layers;
using qon.Machines;
using qon.Variables;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using qon.Exceptions;
using qon.Layers.VariableLayers;

namespace qon.QSL
{
    public static class Filters
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Filter<TQ> GroupByTag<TQ>(string s) where TQ : notnull
        {
            return new Filter<TQ>(v => v.GetNullOrValueProperty(s)
                ?? throw new InternalLogicException($"Object '{v.Name}' is missing required tag '{s}' for grouping."));
        }

        public static Filter<TQ> GroupWith<TIn, TQ>(Func<TIn, object> func) where TIn : ILayer<TQ, QObject<TQ>> where TQ : notnull
        {
            return new Filter<TQ>(v =>
            {
                if (v.LayerManager.GetLayerOrNull<TIn>() is TIn layer)
                {
                    return func(layer);
                }
                else
                {
                    throw new InternalLogicException($"Filter {func} can't be applied to Layer'{typeof(TIn)}'.");
                }
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static QPredicate<TQ> SelectByTagValue<TQ>(string s, object value) where TQ : notnull
        {
            return new QPredicate<TQ>(v => object.Equals(v.GetNullOrValueProperty(s), value));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static QPredicate<TQ> All<TQ>() where TQ : notnull
        {
            return new QPredicate<TQ>(v => true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static QPredicate<TQ> Unassigned<TQ>() where TQ : notnull
        {
            return new QPredicate<TQ>(v => v.OnDomainLayer().State == ValueState.Uncertain);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static QPredicate<TQ> Assigned<TQ>() where TQ : notnull
        {
            return new QPredicate<TQ>(v => v.OnDomainLayer().State != ValueState.Uncertain);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static QPredicate<TQ> DomainContains<TQ>(TQ value) where TQ : notnull
        {
            return new QPredicate<TQ>(v => v.OnDomainLayer().ContainsValue(value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static QPredicate<TQ> EqualsToValue<TQ>(TQ value) where TQ : notnull
        {
            return new QPredicate<TQ>(v => v.OnDomainLayer().State != ValueState.Uncertain && v.Value.CheckValue(value));
        }

        public static QPredicate<TQ> FieldFilter<TQ>(Func<Field<TQ>, bool> func) where TQ : notnull
        {
            return new QPredicate<TQ>(v =>
            {
                var field = v.Machine.Solver.Current.Field;
                return func(field);
            });
        }
    }
}
