using qon.Exceptions;
using qon.Layers.VariableLayers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace qon.Variables.Domains
{
    public static class DomainHelper<T>
    {
        private static readonly Dictionary<Type, Func<QVariable<T>, IDomain<T>, HashSet<T>, int>> HashSetIntersections = new();

        public static void RegisterHashSetIntersection<TDomain>(Func<QVariable<T>, TDomain, HashSet<T>, int> handler)
            where TDomain : IDomain<T>
            => HashSetIntersections[typeof(TDomain)] = (variable, domain, values) => handler(variable, (TDomain)domain, values);

        public static bool TryGetHashSetIntersection(IDomain<T> domain, [NotNullWhen(true)] out Func<QVariable<T>, IDomain<T>, HashSet<T>, int>? func)
            => HashSetIntersections.TryGetValue(domain.GetType(), out func);

        static DomainHelper()
        {
            RegisterHashSetIntersection<DiscreteDomain<T>>(static (_, domain, values) => IntersectDiscreteWithHashSet(domain, values));
            RegisterHashSetIntersection<NumericalDomain<T>>(IntersectNumericalWithHashSet);
            RegisterHashSetIntersection<EmptyDomain<T>>(static (_, __, ___) => 0);
            RegisterHashSetIntersection<PrimitiveDomain<T>>(static (_, domain, values) => IntersectPrimitiveWithHashSet(domain, values));
        }


        public static int DomainIntersectionWithHashSet(QVariable<T> variable, HashSet<T> values)
        {
            var layer = DomainLayer<T>.With(variable);
            var domain = layer.GetDomain();

            var result = TryGetHashSetIntersection(domain, out var handler) 
                ? handler(variable, domain, values) 
                : IntersectDefaultWithHashSet(layer, values);

            if (domain is not EmptyDomain<T> && domain.IsEmpty())
            {
                layer.AssignEmptyDomain();
            }

            return result;
        }

        private static int IntersectDiscreteWithHashSet(DiscreteDomain<T> domain, HashSet<T> values)
        {
            int originalSize = domain.Size();

            foreach (var key in domain.Domain.Keys.ToArray()) if (!values.Contains(key))
            {
                domain.Remove(key);
            }

            return Math.Max(0, originalSize - domain.Size());
        }

        private static int IntersectNumericalWithHashSet(QVariable<T> variable, NumericalDomain<T> domain, HashSet<T> values)
        {
            int originalSize = domain.Size();

#pragma warning disable CS8714
            var filtered = new Dictionary<T, int>(values.Comparer);
#pragma warning restore CS8714

            foreach (var value in values)
            {
                if (domain.ContainsValue(value) && !filtered.ContainsKey(value))
                {
                    filtered[value] = 1;
                }
            }

            var targetLayer = DomainLayer<T>.With(variable);

            if (filtered.Count == 0)
            {
                targetLayer.AssignEmptyDomain();
            }
            else
            {
                targetLayer.AssignDomain(new DiscreteDomain<T>(filtered));
            }

            return originalSize - targetLayer.GetDomain().Size();
        }

        private static int IntersectPrimitiveWithHashSet(PrimitiveDomain<T> domain, HashSet<T> values)
        {
            int originalSize = domain.Size();
            domain.Domain.IntersectWith(values);
            return originalSize - domain.Size();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int IntersectDefaultWithHashSet(DomainLayer<T> layer, HashSet<T> values)
        {
            var originalDomain = layer.GetDomain();

            if (originalDomain is IWeightedDomain<T> weightedOriginalDomain)
            {
                return IntersectDefaultWeightedDomainWithHashSet(layer, weightedOriginalDomain, values);
            }
            else
            {
                return IntersectDefaultDomainWithHashSet(layer, originalDomain, values);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int IntersectDefaultDomainWithHashSet(DomainLayer<T> layer, IDomain<T> domain, HashSet<T> values)
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
#pragma warning disable CS8714
                layer.AssignDomain(new DiscreteDomain<T>(filtered.ToDictionary(pair => pair, pair => 1, values.Comparer)));
#pragma warning restore CS8714
            }

            int originalSize = domain.Size();

            return Math.Max(0, originalSize - layer.GetDomain().Size());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int IntersectDefaultWeightedDomainWithHashSet(DomainLayer<T> layer, IWeightedDomain<T> domain, HashSet<T> values)
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
#pragma warning disable CS8714
                layer.AssignDomain(new DiscreteDomain<T>(filtered.ToDictionary(pair => pair.Key, pair => pair.Value, values.Comparer)));
#pragma warning restore CS8714
            }

            int originalSize = domain.Size();

            return Math.Max(0, originalSize - layer.GetDomain().Size());
        }
    }
}
