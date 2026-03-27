using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using qon.Exceptions;
using qon.Functions;
using qon.Functions.Filters;
using qon.Helpers;
using qon.Layers.VariableLayers;
using qon.Machines;
using qon.Variables;
using qon.Variables.Domains;

namespace qon
{
    public static partial class QSL
    {
        #region Collection Extensions

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] FromNullableToArray<T>(this T? value) where T : class
        {
            return value is { } result ? new[] { result } : Array.Empty<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] FromNullableToArray<T>(this T? value) where T : struct
        {
            return value.HasValue ? new[] { value.Value } : Array.Empty<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T RandomItem<T>(this ICollection<T> collection, Random random)
        {
            int size = collection.Count;

            if (size == 0)
                throw new InternalLogicException("Collection should contain non-zero amount of elements");

            int index = random.Next(size);

            if (collection is List<T> list)
            {
                return list[index];
            }

            if (collection is T[] array)
            {
                return array[index];
            }

            int i = 0;
            foreach (T item in collection)
            {
                if (i == index)
                    return item;

                i++;
            }

            throw new UnreachableException();
        }

        public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this ICollection<T>? collection)
        {
            if (collection is null)
            {
                return true;
            }

            if (collection.Count == 0)
            {
                return true;
            }

            return false;
        }

        public static bool IsNullOrEmpty<TQ>([NotNullWhen(false)] this Field<TQ>? field) where TQ : notnull
        {
            if (field is null)
            {
                return true;
            }

            if (field.Count == 0)
            {
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TValue TryGetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key) where TKey : notnull where TValue : new()
        {
            if (dict.TryGetValue(key, out var value))
            {
                return value;
            }

            value = new();
            dict[key] = value;
            return value;
        }

        #endregion

        #region Functional Extensions

        public static QPredicate<TQ> Or<TQ>(this QPredicate<TQ> left, QPredicate<TQ> right) where TQ : notnull
        {
            var tempFunc = left.PredicateFunction;
            return new QPredicate<TQ>(v => tempFunc(v) || right.PredicateFunction(v));
        }

        public static QPredicate<TQ> And<TQ>(this QPredicate<TQ> left, QPredicate<TQ> right) where TQ : notnull
        {
            var tempFunc = left.PredicateFunction;
            return new QPredicate<TQ>(v => tempFunc(v) && right.PredicateFunction(v));
        }

        public struct WeakHashSet<T> : IEquatable<WeakHashSet<T>>
        {
            public HashSet<T> Set { private set; get; }

            public WeakHashSet(HashSet<T> set)
            {
                Set = set;
            }
            public bool Equals(WeakHashSet<T> other)
            {
                return Set.Intersect(other.Set).Any();
            }

            public override int GetHashCode()
            {
                return -1;
            }

            public override bool Equals(object? obj)
            {
                return obj is WeakHashSet<T> set && Equals(set);
            }
        }

        public static Func<TIn, WeakHashSet<object>> Or<TIn>(this Func<TIn, object> leftFunc, Func<TIn, object> rightFunc)
        {
            WeakHashSet<object> Func(TIn o)
            {
                HashSet<object> set = new HashSet<object> { leftFunc(o), rightFunc(o) };
                return new WeakHashSet<object>(set);
            }

            return Func;
        }

        public static Func<TIn, HashSet<object>> And<TIn>(this Func<TIn, object> leftFunc, Func<TIn, object> rightFunc)
        {
            HashSet<object> Func(TIn o)
            {
                HashSet<object> set = new HashSet<object>
                {
                    leftFunc(o),
                    rightFunc(o)
                };
                return set;
            }

            return Func;
        }

        public static TOut Then<TIn, TOut>(this TIn input, Func<TIn, TOut> func)
        {
            return func(input);
        }

        public static TOut Then<TIn, TOut>(this TIn input, IChain<TIn, TOut> chain)
        {
            return chain.ApplyTo(input);
        }

        public static Func<int, int> Decimation = count =>
        {
            return count switch
            {
                < 10 => 1,
                < 100 => 10,
                < 1000 => 100,
                < 10000 => 1000,
                _ => (int)Math.Log10(count + 1) * 10
            };
        };

        #endregion

        #region Enum Extensions

        public static List<T> ToList<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T)).OfType<T>().ToList();
        }
        #endregion

        #region Random Extensions

        public static bool GetRandomBool(this Random random, double probability = 0.5)
        {
            return random.NextDouble() < probability;
        }

        public static Guid GetRandomGuid(this Random random)
        {
            byte[] guidBytes = new byte[16];
            random.NextBytes(guidBytes);
            return new Guid(guidBytes);
        }

        #endregion

        #region Object

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T NewOrExisting<T>(this T? obj) where T: new()
        {
            return obj ?? new T();
        }

        #endregion

        #region String Extension

        public static string ToShortString(this string str, int limit) 
        {
            if (str.Length <= limit)
            {
                return str;
            }
            return str.Substring(0, limit) + "...";
        }

        public static bool IsNullOrEmpty([NotNullWhen(false)] this string? str)
        {
            return string.IsNullOrEmpty(str);
        }

        #endregion

        #region Variables Extensions

        public static QLink<TQ> WithDomain<TQ>(this QLink<TQ> link, IDomain<TQ> domain) where TQ : notnull
        {
            var d = DomainLayer<TQ>.GetOrCreate(link.Object);
            d.AssignDomain(domain);

            return link;
        }

        public static QLink<TQ> WithValue<TQ>(this QLink<TQ> link, TQ value) where TQ : notnull
        {
            link.Object.Value = value;

            return link;
        }

        public static Action<QObject<TQ>> WithDomain<TQ>(IDomain<TQ> domain) where TQ: notnull
        {
            return (variable) =>
            {
                var d = DomainLayer<TQ>.GetOrCreate(variable);
                d.AssignDomain(domain);
            };
        }

        #endregion
    }
}
