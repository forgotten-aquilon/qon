using qon.Exceptions;
using qon.Layers.VariableLayers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace qon.Variables.Domains
{
    public static class DomainHelper
    {
        #region Pre-built Domains

        public class CharDomainOptions
        {
            public HashSet<char> Symbols { get; } = new();

            //FUTURE: add check for same script symbols
            public CharDomainOptions WithAlphabet(char leftSymbol, char rightSymbol)
            {
                var validatedLeftSymbol = ExceptionHelper.ThrowIfPredicateFalse(leftSymbol, symbol => char.IsLetter(leftSymbol));
                var validatedRightSymbol = ExceptionHelper.ThrowIfPredicateFalse(rightSymbol, symbol => char.IsLetter(rightSymbol));

                if (char.IsLower(validatedLeftSymbol) != char.IsLower(validatedRightSymbol))
                {
                    throw new ValidationException($"Symbols '{validatedLeftSymbol}' and '{validatedRightSymbol}' should be of the same case");
                }

                var diff = ExceptionHelper.ThrowIfPredicateFalse(validatedRightSymbol - validatedLeftSymbol, diff => diff > 0);

                Symbols.UnionWith(Enumerable.Range(validatedLeftSymbol, diff + 1).Select(n => (char)n));

                return this;
            }

            public CharDomainOptions WithDigits(char leftSymbol, char rightSymbol)
            {
                var validatedLeftSymbol = ExceptionHelper.ThrowIfPredicateFalse(leftSymbol, symbol => char.IsDigit(leftSymbol));
                var validatedRightSymbol = ExceptionHelper.ThrowIfPredicateFalse(rightSymbol, symbol => char.IsDigit(rightSymbol));

                var diff = ExceptionHelper.ThrowIfPredicateFalse(validatedRightSymbol - validatedLeftSymbol, diff => diff > 0);

                Symbols.UnionWith(Enumerable.Range(validatedLeftSymbol, diff + 1).Select(n => (char)n));

                return this;
            }

            public CharDomainOptions WithOtherSymbols(params char[] symbols)
            {
                Symbols.UnionWith(symbols);
                return this;
            }
        }

        public static PrimitiveDomain<char> SymbolicalDomain(CharDomainOptions options)
        {
            return new PrimitiveDomain<char>(options.Symbols);
        }

        public static PrimitiveDomain<int> SimpleNumericalDomain(int fromInclusive, int toInclusive)
        {
            ExceptionHelper.ThrowIfPredicateFalse(toInclusive - fromInclusive, diff => diff >= 0);
            var symbols = Enumerable.Range(fromInclusive, toInclusive - fromInclusive + 1).ToHashSet();
            return new PrimitiveDomain<int>(symbols);
        }

        //FUTURE: God, I really need INumber<T>, Unity, WHEN???
        public static NumericalDomain<int> NumericalDomain(params (int, int)[] ranges)
        {
            if (ranges.Length == 0)
            {
                return new NumericalDomain<int>();
            }
            else
            {
                var validatedRanges = ExceptionHelper.ThrowIfPredicateFalse(ranges, rngs => rngs.Any(range => range.Item2 - range.Item1 < 1));
                return new NumericalDomain<int>(validatedRanges.Select(x => new Interval<int>(x.Item1, x.Item2)));
            }
        }

        public static NumericalDomain<uint> NumericalDomain(params (uint, uint)[] ranges)
        {
            if (ranges.Length == 0)
            {
                return new NumericalDomain<uint>();
            }
            else
            {
                var validatedRanges = ExceptionHelper.ThrowIfPredicateFalse(ranges, rngs => rngs.Any(range => range.Item2 - range.Item1 < 1));
                return new NumericalDomain<uint>(validatedRanges.Select(x => new Interval<uint>(x.Item1, x.Item2)));
            }
        }

        public static NumericalDomain<long> NumericalDomain(params (long, long)[] ranges)
        {
            if (ranges.Length == 0)
            {
                return new NumericalDomain<long>();
            }
            else
            {
                var validatedRanges = ExceptionHelper.ThrowIfPredicateFalse(ranges, rngs => rngs.Any(range => range.Item2 - range.Item1 < 1));
                return new NumericalDomain<long>(validatedRanges.Select(x => new Interval<long>(x.Item1, x.Item2)));
            }
        }

        public static NumericalDomain<ulong> NumericalDomain(params (ulong, ulong)[] ranges)
        {
            if (ranges.Length == 0)
            {
                return new NumericalDomain<ulong>();
            }
            else
            {
                var validatedRanges = ExceptionHelper.ThrowIfPredicateFalse(ranges, rngs => rngs.Any(range => range.Item2 - range.Item1 < 1));
                return new NumericalDomain<ulong>(validatedRanges.Select(x => new Interval<ulong>(x.Item1, x.Item2)));
            }
        }

        #endregion
    }

    public static class DomainHelper<TQ> where TQ : notnull
    {
        private static readonly Dictionary<Type, Func<QVariable<TQ>, IDomain<TQ>, HashSet<TQ>, int>> HashSetIntersections = new();

        public static void RegisterHashSetIntersection<TDomain>(Func<QVariable<TQ>, TDomain, HashSet<TQ>, int> handler)
            where TDomain : IDomain<TQ>
            => HashSetIntersections[typeof(TDomain)] = (variable, domain, values) => handler(variable, (TDomain)domain, values);

        public static bool TryGetHashSetIntersection(IDomain<TQ> domain, [NotNullWhen(true)] out Func<QVariable<TQ>, IDomain<TQ>, HashSet<TQ>, int>? func)
            => HashSetIntersections.TryGetValue(domain.GetType(), out func);

        static DomainHelper()
        {
            RegisterHashSetIntersection<DiscreteDomain<TQ>>(static (_, domain, values) => IntersectDiscreteWithHashSet(domain, values));
            RegisterHashSetIntersection<NumericalDomain<TQ>>(IntersectNumericalWithHashSet);
            RegisterHashSetIntersection<EmptyDomain<TQ>>(static (_, __, ___) => 0);
            RegisterHashSetIntersection<PrimitiveDomain<TQ>>(static (_, domain, values) => IntersectPrimitiveWithHashSet(domain, values));
        }

        #region Domain intersections with HashSet

        public static int DomainIntersectionWithHashSet(QVariable<TQ> variable, HashSet<TQ> values)
        {
            var layer = DomainLayer<TQ>.With(variable);
            var domain = layer.GetDomain();

            var result = TryGetHashSetIntersection(domain, out var handler) 
                ? handler(variable, domain, values) 
                : IntersectDefaultWithHashSet(layer, values);

            if (domain is not EmptyDomain<TQ> && domain.IsEmpty())
            {
                layer.AssignEmptyDomain();
            }

            return result;
        }

        private static int IntersectDiscreteWithHashSet(DiscreteDomain<TQ> domain, HashSet<TQ> values)
        {
            int originalSize = domain.Size();

            foreach (var key in domain.Domain.Keys.ToArray()) if (!values.Contains(key))
            {
                domain.Remove(key);
            }

            return Math.Max(0, originalSize - domain.Size());
        }

        private static int IntersectNumericalWithHashSet(QVariable<TQ> variable, NumericalDomain<TQ> domain, HashSet<TQ> values)
        {
            int originalSize = domain.Size();
            var filtered = new Dictionary<TQ, int>(values.Comparer);

            foreach (var value in values)
            {
                if (domain.ContainsValue(value) && !filtered.ContainsKey(value))
                {
                    filtered[value] = 1;
                }
            }

            var targetLayer = DomainLayer<TQ>.With(variable);

            if (filtered.Count == 0)
            {
                targetLayer.AssignEmptyDomain();
            }
            else
            {
                targetLayer.AssignDomain(new DiscreteDomain<TQ>(filtered));
            }

            return originalSize - targetLayer.GetDomain().Size();
        }

        private static int IntersectPrimitiveWithHashSet(PrimitiveDomain<TQ> domain, HashSet<TQ> values)
        {
            int originalSize = domain.Size();
            domain.Domain.IntersectWith(values);
            return originalSize - domain.Size();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int IntersectDefaultWithHashSet(DomainLayer<TQ> layer, HashSet<TQ> values)
        {
            var originalDomain = layer.GetDomain();

            if (originalDomain is IWeightedDomain<TQ> weightedOriginalDomain)
            {
                return IntersectDefaultWeightedDomainWithHashSet(layer, weightedOriginalDomain, values);
            }
            else
            {
                return IntersectDefaultDomainWithHashSet(layer, originalDomain, values);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int IntersectDefaultDomainWithHashSet(DomainLayer<TQ> layer, IDomain<TQ> domain, HashSet<TQ> values)
        {
            var filtered = domain
                .GetValues()
                .Where(values.Contains)
                .ToList();

            if (filtered.Count == 0)
            {
                layer.AssignEmptyDomain();
            }
            else
            {
                layer.AssignDomain(new DiscreteDomain<TQ>(filtered.ToDictionary(pair => pair, pair => 1, values.Comparer)));
            }

            int originalSize = domain.Size();

            return Math.Max(0, originalSize - layer.GetDomain().Size());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int IntersectDefaultWeightedDomainWithHashSet(DomainLayer<TQ> layer, IWeightedDomain<TQ> domain, HashSet<TQ> values)
        {
            var filtered = domain
                .GetValuesWithWeights()
                .Where(pair => values.Contains(pair.Key))
                .ToList();

            if (filtered.Count == 0)
            {
                layer.AssignEmptyDomain();
            }
            else
            {
                layer.AssignDomain(new DiscreteDomain<TQ>(filtered.ToDictionary(pair => pair.Key, pair => pair.Value, values.Comparer)));
            }

            int originalSize = domain.Size();

            return Math.Max(0, originalSize - layer.GetDomain().Size());
        }

        #endregion
    }
}
