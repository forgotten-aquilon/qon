using System;

namespace qon
{
    public class OptionalHasNoValueException : Exception
    {

    }

    public struct Optional<T> : ICloneable
    {
        //TODO: Update to C# 10 when available (default)
        private T? _value;

        //TODO: Update to C# 10 when available (default)
        public bool HasValue { get; }

        public T Value
        {
            get
            {
                if (_value is null)
                    throw new OptionalHasNoValueException();

                return _value;
            }
        }

        public static Optional<T> Empty => new();

        public Optional(T value)
        {
            if (value is null)
            {
                this = Empty;
                HasValue = false;
                return;
            }

            _value = value;
            HasValue = true;
        }

        public bool CheckValue(T value)
        {
#pragma warning disable CS8602
            return HasValue && Value.Equals(value);
#pragma warning restore CS8602 
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
        public Optional<T> Copy()
        {
            return (Optional<T>)Clone();
        }

        public override int GetHashCode()
        {
#pragma warning disable CS8602
            return !HasValue ? 0 : Value.GetHashCode();
#pragma warning restore CS8602
        }

        public static bool operator ==(Optional<T> left, Optional<T> right)
        {
            if (!left.HasValue && !right.HasValue)
            {
                return true;
            }

            if (left.HasValue != right.HasValue)
            {
                return false;
            }

#pragma warning disable CS8602
            return left.Value.Equals(right.Value);
#pragma warning restore CS8602
        }

        public static bool operator !=(Optional<T> left, Optional<T> right)
        {
            return !(left == right);
        }

        public override bool Equals(object? obj)
        {
            if (obj is Optional<T> right)
            {
                return this == right;
            }
            return false;
        }

        public override string ToString()
        {
#pragma warning disable CS8602, CS8603
            return HasValue ? Value.ToString() : nameof(Optional<T>);
#pragma warning restore CS8602, CS8603
        }
    }
}
