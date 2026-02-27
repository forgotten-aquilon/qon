using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace qon.Helpers
{
    public class OptionalHasNoValueException : Exception
    {

    }

    /// <summary>
    /// Internal implementation of Either/Optional monad
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct Optional<T> : ICloneable, ICopy<Optional<T>>, IEquatable<Optional<T>>
    {
        private static readonly EqualityComparer<T> Comparer = EqualityComparer<T>.Default;

        //TODO: Future: Update to C# 10 when available (default)
        private readonly T? _value;

        public bool HasValue { get; }

        public readonly T Value
        {
            get
            {
                if (_value is null)
                    throw new OptionalHasNoValueException();

                return _value;
            }
        }

        public readonly Type Type => typeof(T);

        public static Optional<T> Empty => new();

        private Optional(T? value)
        {
            if (value is null)
            {
                this = Empty;
                HasValue = false;
                return;
            }

            HasValue = true;
            _value = value;
        }

        /// <summary>
        /// TODO: write
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckValue(in T value)
        {
            return HasValue && Comparer.Equals(Value, value);
        }

        public bool TryGetValue(out T? value)
        {
            if (HasValue)
            {
                value = _value;
                return true;
            }

            value = default;
            return false;
        }

        public object Clone()
        {
            return _value is not null ? new Optional<T>(_value) : Optional<T>.Empty;
        }

        /// <summary>
        /// Use Optional&lt;T&gt; a = b.Copy() instead of Optional&lt;T&gt; a = b;
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Optional<T> Copy()
        {
            return (Optional<T>)Clone();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return !HasValue ? 0 : Value!.GetHashCode();
        }

        #region IEquatable

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Optional<T> other)
        {
            if (!HasValue && !other.HasValue)
            {
                return true;
            }

            if (HasValue == !other.HasValue)
            {
                return false;
            }

            return Comparer.Equals(Value, other.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj)
        {
            if (obj is Optional<T> right)
            {
                return this.Equals(right);
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Optional<T> left, Optional<T> right)
        {
            return left.Equals(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Optional<T> left, Optional<T> right)
        {
            return !(left == right);
        }

        #endregion

        public override string ToString()
        {
            return HasValue ? Value!.ToString()! : nameof(Optional<T>);
        }

        public static Optional<T> Of(T value)
        {
            return new Optional<T>(value);
        }
    }
}
