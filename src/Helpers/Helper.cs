using qon.Exceptions;
using qon.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using qon.Functions.Filters;

namespace qon.Helpers
{
    public static class Helper
    {
        #region Collection Extensions

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] FromNullableToArray<T>(this T? value) where T : class
        {
            return value is T result ? new T[] { result } : Array.Empty<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] FromNullableToArray<T>(this T? value) where T : struct
        {
            return value.HasValue ? new T[] { value.Value } : Array.Empty<T>();
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

        public static QPredicate<T> Or<T>(this QPredicate<T> left, QPredicate<T> right)
        {
            var tempFunc = left.PredicateFunction;
            return new QPredicate<T>(v => tempFunc(v) || right.PredicateFunction(v));
        }

        public static QPredicate<T> And<T>(this QPredicate<T> left, QPredicate<T> right)
        {
            var tempFunc = left.PredicateFunction;
            return new QPredicate<T>(v => tempFunc(v) && right.PredicateFunction(v));
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
                return obj is WeakHashSet<T> && Equals((WeakHashSet<T>)obj);
            }
        }

        public static Func<TIn, WeakHashSet<object>> Or<TIn>(this Func<TIn, object> leftFunc, Func<TIn, object> rightFunc)
        {
            Func<TIn, WeakHashSet<object>> func = o =>
            {
                HashSet<object> set = new HashSet<object>
                {
                    leftFunc(o),
                    rightFunc(o)
                };
                return new WeakHashSet<object>(set);
            };

            return func;
        }

        public static Func<TIn, HashSet<object>> And<TIn>(this Func<TIn, object> leftFunc, Func<TIn, object> rightFunc)
        {
            Func<TIn, HashSet<object>> func = o =>
            {
                HashSet<object> set = new HashSet<object>
                {
                    leftFunc(o),
                    rightFunc(o)
                };
                return set;
            };

            return func;
        }

        #endregion

        #region Enum Extensions

        public static List<T> ToList<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T)).OfType<T>().ToList();
        }
        #endregion
    }
}
