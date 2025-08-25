using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace qon.Domains
{
    public enum Operation
    {
        Increment,
        Decrement,
    }

    public struct Interval<T>
    {
        public T Start { get; set; }
        public T End { get; set; }
        public UInt64 Length
        {
            get
            {
                try
                {
                    checked
                    {
                        Int64 end = Convert.ToInt64(End);
                        Int64 start = Convert.ToInt64(Start);

                        if (end >= start)
                        {
                            unchecked
                            {
                                return (UInt64)(end) + (UInt64)(-start) + 1;
                            }
                        }

                        throw new InternalLogicException("Unable to cast a number within range of Int64");
                    }
                }
                catch (OverflowException)
                {
                    UInt64 end = Convert.ToUInt64(End);
                    UInt64 start = Convert.ToUInt64(Start);

                    if (end >= start)
                    {
                        unchecked
                        {
                            return end - start + 1;
                        }
                    }

                    throw new InternalLogicException("Unable to cast a number within range of UInt64");
                }
            }
        }

        public Interval(T start, T end)
        {
            Start = start;
            End = end;
        }

        public Interval((T minValue, T maxValue) value) : this(value.minValue, value.maxValue)
        {
        }

        public bool ContainsValue(T value)
        {
            if (NumericalDomain<T>.Compare(Start, End) == 0 && NumericalDomain<T>.Compare(Start, value) == 0)
                return true;

            var leftCheck = NumericalDomain<T>.Compare(Start, value);

            var rightCheck = NumericalDomain<T>.Compare(End, value);

            return rightCheck > leftCheck;
        }

        public override string ToString() => $"[{Start}..{End}]";
    }


    public class NumericalDomain<T> : IDomain<T> //TODO: Refactor this shit with INumber<T> as soon as available in Unity3D
    {
        private readonly bool _isSigned = true;
        private readonly Type _type = typeof(T);

        public readonly List<Interval<T>> Domain = new();

        public NumericalDomain()
        {
            TypeCode typeCode = Type.GetTypeCode(typeof(T));

            if (typeCode is < TypeCode.Boolean or > TypeCode.UInt64)
            {
                throw new InternalLogicException($"{typeof(T)} should be bool or integer");
            }

            //Typecodes of unsigned numerical types
            if ((int)typeCode % 2 == 0)
            {
                _isSigned = false;
            }

            Domain.Add(new Interval<T>(GetLimits()));
        }

        public NumericalDomain(IEnumerable<Interval<T>> intervals)
        {
            TypeCode typeCode = Type.GetTypeCode(typeof(T));

            if (typeCode is < TypeCode.Boolean or > TypeCode.UInt64)
            {
                throw new InternalLogicException($"{typeof(T)} should be bool or integer");
            }

            //Typecodes of unsigned numerical types
            if ((int)typeCode % 2 == 0)
            {
                _isSigned = false;
            }

            Domain = intervals.Select(x => x).ToList();
        }

        public UInt64 TrueSize()
        {
            return Domain.Select(x => x.Length).Aggregate((a, b) => a+b);
        }

        #region IDomain<T> interface

        public int Size()
        {
            if (IsEmpty())
            {
                return 0;
            }

            int size = 0;
            const UInt64 limit = int.MaxValue;

            foreach (var interval in Domain)
            {
                UInt64 length = interval.Length;

                if (length > limit)
                {
                    return int.MaxValue;
                }

                try
                {
                    checked
                    {
                        size += (int)interval.Length;
                    }
                }
                catch (OverflowException)
                {
                    return int.MaxValue;
                }
            }

            return size;
        }

        public bool IsEmpty()
        {
            return Domain.Count == 0;
        }

        public bool ContainsValue(T value)
        {
            return GetItemIndex(value) > -1;
        }

        public int Remove(T item)
        {
            int position = GetItemIndex(item);

            if (position == -1)
            {
                return 0;
            }

            Interval<T> interval = Domain[position];

            if (Compare(interval.Start, interval.End) == 0)
            {
                Domain.RemoveAt(position);
            }
            else if (Compare(interval.End, item) == 0)
            {
                interval.End = UnaryOperation(item, Operation.Decrement);
                Domain[position] = interval;
            }
            else if (Compare(interval.Start, item) == 0)
            {
                interval.Start = UnaryOperation(item, Operation.Increment);
                Domain[position] = interval;
            }
            else
            {
                var leftInterval = new Interval<T>(interval.Start, UnaryOperation(item, Operation.Decrement));
                var rightInterval = new Interval<T>(UnaryOperation(item, Operation.Increment), interval.End);
                Domain[position] = leftInterval;
                Domain.Insert(position+1, rightInterval);
            }

            return 1;
        }

        public int Remove(IEnumerable<T> items)
        {
            int changeCount = 0;

            foreach (var v in items.Distinct().OrderBy(x => x))
                changeCount += Remove(v);
            
            return changeCount;
        }

        public void Clear()
        {
            Domain.Clear();
        }

        public double GetEntropy()
        {
            return Math.Log(TrueSize(), 2);
        }

        //TODO: C# 10 supports random long
        public T GetRandomValue(Random random)
        {
            UInt64 topProbabilityLimit = TrueSize();
            Span<byte> bytes = stackalloc byte[8];
            random.NextBytes(bytes);

            UInt64 thresholdProbability = BitConverter.ToUInt64(bytes) % topProbabilityLimit;

            UInt64 probability = 0;
            foreach (var interval in Domain)
            {
                probability += interval.Length;

                if (probability > thresholdProbability)
                {
                    return UnaryOperation(interval.Start, Operation.Increment, probability - thresholdProbability - 1);
                }
            }

            throw new InternalLogicException("");
        }

        public Optional<T> SingleOrEmptyValue()
        {
            if (Size() == 1)
            {
                return new Optional<T>(Domain[0].Start);
            }

            return Optional<T>.Empty;
        }

        public IDomain<T> Copy()
        {
            return new NumericalDomain<T>(Domain);
        }

        public IEnumerable<KeyValuePair<T, int>> GetIEnumerable()
        {
            foreach (var r in Domain)
            {
                for (T x = r.Start; ; x = UnaryOperation(x, Operation.Increment))
                {
                    yield return new KeyValuePair<T, int>(x, 1);
                    if (Compare(x, r.End) == 0) 
                        break;
                }
            }
        }

        #endregion
        public override string ToString()
        {
            if (IsEmpty())
            {
                return "[..]";
            }

            return $"[{Domain[0].Start}..{Domain[^1].End}]";
        }

        #region Helpers

        //TODO: Holy fuck, redo this shit, when will be able to work with generic math via INumber<T>
        private static (T minValue, T maxValue) GetLimits()
        {
            var innerType = typeof(T);
            var minField = innerType.GetField("MinValue", BindingFlags.Public | BindingFlags.Static);
            var maxField = innerType.GetField("MaxValue", BindingFlags.Public | BindingFlags.Static);

            if (minField is not null && maxField is not null)
            {
                return ((T)minField.GetValue(null)!, (T)maxField.GetValue(null)!);
            }

            throw new InternalLogicException($"{typeof(T)} does not support MinValue and MaxValue fields");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns>-1 if there is no interval with such value, returns index of interval from _intervals collection</returns>
        /// <exception cref="InternalNullException"></exception>
        private int GetItemIndex(T value)
        {
            if (IsEmpty())
            {
                return -1;
            }

            //Binary search
            int low = 0;
            int high = Domain.Count - 1;
            int mid = Domain.Count / 2;

            while (!Domain[mid].ContainsValue(value) && low <= high)
            {
                if (Compare(value, Domain[mid].End) > 0)
                {
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
                mid = (low + high) / 2;
            }

            if (low > high)
            {
                return -1;
            }
            else
            {
                return mid;
            }
        }

        private T UnaryOperation(T value, Operation operation, UInt64 operand = 1)
        {
            if (_isSigned)
            {
                Int64 number = Convert.ToInt64(value);
                switch (operation)
                {
                    case Operation.Increment:
                        return (T)Convert.ChangeType(number + (Int64)operand, _type);
                    case Operation.Decrement:
                        return (T)Convert.ChangeType(number - (Int64)operand, _type);
                }
            }
            else
            {
                UInt64 number = Convert.ToUInt64(value);
                switch (operation)
                {
                    case Operation.Increment:
                        return (T)Convert.ChangeType(number + operand, _type);
                    case Operation.Decrement:
                        return (T)Convert.ChangeType(number - operand, _type);
                }
            }

            throw new InternalLogicException("Exception!");
        }

        #endregion

        public static int Compare(T obj1, T obj2)
        {
            var innerType = typeof(T);
            var compareMethod = innerType.GetMethod("CompareTo", new[] { innerType });

            if (obj1 == null)
            {
                throw new ArgumentNullException(nameof(obj1));
            }

            if (obj2 == null)
            {
                throw new ArgumentNullException(nameof(obj2));
            }

            if (compareMethod is null)
            {
                throw new InternalNullException(nameof(compareMethod));
            }

#pragma warning disable CS8605
            return (int)(compareMethod.Invoke(obj1, new object[] { obj2 }));
#pragma warning restore CS8605
        }

        public static T Min(T obj1, T obj2)
        {
            return (Compare(obj1, obj2) < 0) ? obj1 : obj2;
        }

        public static T Max(T obj1, T obj2)
        {
            return (Compare(obj1, obj2) > 0) ? obj1 : obj2;
        }
    }
}
