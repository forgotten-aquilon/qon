using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using qon.Exceptions;
using qon.Helpers;

namespace qon.Variables.Domains
{
    public enum Operation
    {
        Increment,
        Decrement,
    }

    public struct Interval<TQ> where TQ : notnull
    {
        public TQ Start { get; set; }
        public TQ End { get; set; }

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

        public Interval(TQ start, TQ end)
        {
            Start = start;
            End = end;
        }

        public Interval((TQ minValue, TQ maxValue) value) : this(value.minValue, value.maxValue)
        {
        }

        public bool ContainsValue(TQ value)
        {
            if (NumericalDomain<TQ>.Compare(Start, End) == 0 && NumericalDomain<TQ>.Compare(Start, value) == 0)
                return true;

            var leftCheck = NumericalDomain<TQ>.Compare(Start, value);

            var rightCheck = NumericalDomain<TQ>.Compare(End, value);

            return rightCheck > leftCheck;
        }

        public override string ToString() => $"[{Start}..{End}]";
    }

    public class NumericalDomain<TQ> : IDomain<TQ> where TQ : notnull //FUTURE: Refactor this shit with INumber<TQ> as soon as available in Unity3D
    {
        private static readonly IComparer<TQ> Comparer = Comparer<TQ>.Default;

        private readonly bool _isSigned = true;
        private readonly Type _type = typeof(TQ);

        public readonly List<Interval<TQ>> Domain = new();

        public NumericalDomain()
        {
            TypeCode typeCode = Type.GetTypeCode(typeof(TQ));

            if (typeCode is < TypeCode.Boolean or > TypeCode.UInt64)
            {
                throw new InternalLogicException($"{typeof(TQ)} should be bool or integer");
            }

            //Typecodes of unsigned numerical types
            if ((int)typeCode % 2 == 0)
            {
                _isSigned = false;
            }

            Domain.Add(new Interval<TQ>(GetLimits()));
        }

        public NumericalDomain(IEnumerable<Interval<TQ>> intervals)
        {
            TypeCode typeCode = Type.GetTypeCode(typeof(TQ));

            if (typeCode is < TypeCode.Boolean or > TypeCode.UInt64)
            {
                throw new InternalLogicException($"{typeof(TQ)} should be bool or integer");
            }

            //Typecodes of unsigned numerical types
            if ((int)typeCode % 2 == 0)
            {
                _isSigned = false;
            }

            Domain = intervals.Select(x => x).ToList();
        }

        #region IDomain<TQ> interface

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEmpty()
        {
            return Domain.Count == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsValue(TQ value)
        {
            return GetItemIndex(value) > -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Remove(TQ item)
        {
            int position = GetItemIndex(item);

            if (position == -1)
            {
                return 0;
            }

            Interval<TQ> interval = Domain[position];

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
                var leftInterval = new Interval<TQ>(interval.Start, UnaryOperation(item, Operation.Decrement));
                var rightInterval = new Interval<TQ>(UnaryOperation(item, Operation.Increment), interval.End);
                Domain[position] = leftInterval;
                Domain.Insert(position + 1, rightInterval);
            }

            return 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Remove(IEnumerable<TQ> items)
        {
            int changeCount = 0;

            foreach (var v in items.Distinct().OrderBy(x => x))
                changeCount += Remove(v);

            return changeCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Domain.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetEntropy()
        {
            if (IsEmpty())
                throw new InternalLogicException("Should not be called in this case");

            return Math.Log(TrueSize(), 2);
        }

        //FUTURE: C# 10 supports random long
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TQ GetRandomValue(Random random)
        {
            UInt64 topProbabilityLimit = TrueSize();

            if (topProbabilityLimit == 0 || IsEmpty())
                throw new InternalLogicException("Domain should have at least one non-zero weighted value");

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

            throw new UnreachableException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Optional<TQ> SingleOrEmptyValue()
        {
            if (Size() == 1)
            {
                return Domain[0].Start;
            }

            return Optional<TQ>.Empty;
        }

        public IDomain<TQ> Copy()
        {
            return new NumericalDomain<TQ>(Domain);
        }

        public IEnumerable<TQ> GetValues()
        {
            foreach (var r in Domain)
            {
                for (TQ x = r.Start;; x = UnaryOperation(x, Operation.Increment))
                {
                    yield return x;
                    if (Compare(x, r.End) == 0)
                        break;
                }
            }
        }

        //public IEnumerable<KeyValuePair<TQ, int>> GetValuesWithWeights()
        //{
        //    foreach (var r in Domain)
        //    {
        //        for (TQ x = r.Start;; x = UnaryOperation(x, Operation.Increment))
        //        {
        //            yield return new KeyValuePair<TQ, int>(x, 1);
        //            if (Compare(x, r.End) == 0)
        //                break;
        //        }
        //    }
        //}

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

        //FUTURE: Holy fuck, redo this shit, when will be able to work with generic math via INumber<T>
        private static (TQ minValue, TQ maxValue) GetLimits()
        {
            var innerType = typeof(TQ);
            var minField = innerType.GetField("MinValue", BindingFlags.Public | BindingFlags.Static);
            var maxField = innerType.GetField("MaxValue", BindingFlags.Public | BindingFlags.Static);

            if (minField is not null && maxField is not null)
            {
                return ((TQ)minField.GetValue(null)!, (TQ)maxField.GetValue(null)!);
            }

            throw new InternalLogicException($"{typeof(TQ)} does not support MinValue and MaxValue fields");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns>-1 if there is no interval with such value, returns index of interval from _intervals collection</returns>
        /// <exception cref="InternalNullException"></exception>
        private int GetItemIndex(TQ value)
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

        private TQ UnaryOperation(TQ value, Operation operation, UInt64 operand = 1)
        {
            if (_isSigned)
            {
                Int64 number = Convert.ToInt64(value);
                return operation switch
                {
                    Operation.Increment => (TQ)Convert.ChangeType(number + (Int64)operand, _type),
                    Operation.Decrement => (TQ)Convert.ChangeType(number - (Int64)operand, _type),
                    _ => throw new NonExhaustiveExpressionException(operation)
                };
            }
            else
            {
                UInt64 number = Convert.ToUInt64(value);
                return operation switch
                {
                    Operation.Increment => (TQ)Convert.ChangeType(number + operand, _type),
                    Operation.Decrement => (TQ)Convert.ChangeType(number - operand, _type),
                    _ => throw new NonExhaustiveExpressionException(operation)
                };
            }
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UInt64 TrueSize()
        {
            if (Domain.Count == 0)
                return 0;

            return Domain.Select(x => x.Length).Aggregate((a, b) => a + b);
        }

        public static int Compare(TQ leftObject, TQ rightObject)
        {
            return Comparer.Compare(leftObject, rightObject);
        }

        public static TQ Min(TQ obj1, TQ obj2)
        {
            return (Compare(obj1, obj2) < 0) ? obj1 : obj2;
        }

        public static TQ Max(TQ obj1, TQ obj2)
        {
            return (Compare(obj1, obj2) > 0) ? obj1 : obj2;
        }
    }
}