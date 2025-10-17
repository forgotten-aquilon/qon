using qon.Exceptions;
using qon.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using qon.Layers.VariableLayers;

namespace qon.Domains
{
    public static class DomainHelper<T>
    {
        private static readonly Dictionary<(Type, Type), Func<IDomain<T>, IDomain<T>, IDomain<T>>> Map = new();
        private static readonly Dictionary<Type, Func<QVariable<T>, IDomain<T>, HashSet<T>, int>> HashSetIntersections = new();

        public static void Register<T1, T2, TOut>(Func<T1, T2, TOut> handler) where T1: IDomain<T> where T2: IDomain<T> where TOut: IDomain<T>
            => Map[(typeof(T1), typeof(T2))] = (a, b) => handler((T1)a, (T2)b);

        public static bool TryGet(IDomain<T> a, IDomain<T> b, out Func<IDomain<T>, IDomain<T>, IDomain<T>>? func)
            => Map.TryGetValue((a.GetType(), b.GetType()), out func);

        public static void RegisterHashSetIntersection<TDomain>(Func<QVariable<T>, TDomain, HashSet<T>, int> handler)
            where TDomain : IDomain<T>
            => HashSetIntersections[typeof(TDomain)] = (variable, domain, values) => handler(variable, (TDomain)domain, values);

        public static bool TryGetHashSetIntersection(IDomain<T> domain, out Func<QVariable<T>, IDomain<T>, HashSet<T>, int>? func)
            => HashSetIntersections.TryGetValue(domain.GetType(), out func);

        static DomainHelper()
        {
            Register<DiscreteDomain<T>, DiscreteDomain<T>, IDomain<T>>(DiscreteDomainIntersection);
            Register<NumericalDomain<T>, NumericalDomain<T>, IDomain<T>>(NumericalDomainIntersection);
            Register<DiscreteDomain<T>, NumericalDomain<T>, IDomain<T>>(DiscreteNumericalDomainIntersection);
            Register<NumericalDomain<T>, DiscreteDomain<T>, IDomain<T>>(NumericalDiscreteDomainIntersection);

            RegisterHashSetIntersection<DiscreteDomain<T>>(static (_, domain, values) => IntersectDiscreteWithHashSet(domain, values));
            RegisterHashSetIntersection<NumericalDomain<T>>(IntersectNumericalWithHashSet);
            RegisterHashSetIntersection<EmptyDomain<T>>(static (_, __, ___) => 0);
        }

        //Resulted domain uses weights from the first intersected domain
        public static IDomain<T> DomainIntersection(IDomain<T> domain1, IDomain<T> domain2)
        {
            if (TryGet(domain1, domain2, out var func))
            {
                return func!(domain1, domain2);
            }

            if (TryGet(domain2, domain1, out var symmetricFunc))
            {
                return symmetricFunc!(domain2, domain1);
            }

            throw new InternalLogicException($"There is no method for intersection of domains of types {domain1.GetType()} and {domain2.GetType()}. Register it using Register method of DomainHelper class");
        }

        public static int DomainIntersectionWithHashSet(QVariable<T> variable, HashSet<T> values)
        {
            var layer = SuperpositionLayer<T>.With(variable);
            var domain = layer.Domain;

            if (TryGetHashSetIntersection(domain, out var handler))
            {
                return handler!(variable, domain, values);
            }

            return IntersectGenericWithHashSet(variable, values);
        }

        private static int IntersectDiscreteWithHashSet(DiscreteDomain<T> domain, HashSet<T> values)
        {
            int originalSize = SafeSize(domain);

            foreach (var key in domain.Domain.Keys.ToArray()) if (!values.Contains(key))
            {
                domain.Remove(key);
            }

            return Math.Max(0, originalSize - SafeSize(domain));
        }

        private static int IntersectNumericalWithHashSet(QVariable<T> variable, NumericalDomain<T> domain, HashSet<T> values)
        {
            int originalSize = SafeSize(domain);

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

            var targetLayer = SuperpositionLayer<T>.With(variable);
            targetLayer.Domain = filtered.Count == 0
                ? EmptyDomain<T>.Instance
                : new DiscreteDomain<T>(filtered);

            return Math.Max(0, originalSize - SafeSize(targetLayer.Domain));
        }

        private static int IntersectGenericWithHashSet(QVariable<T> variable, HashSet<T> values)
        {
            var layer = SuperpositionLayer<T>.With(variable);
            var originalDomain = layer.Domain;
            int originalSize = SafeSize(originalDomain);

            var filtered = originalDomain
                .GetIEnumerable()
                .Where(pair => values.Contains(pair.Key))
                .ToList();

            if (filtered.Count == 0)
            {
                layer.Domain = EmptyDomain<T>.Instance;
            }
            else
            {
#pragma warning disable CS8714
                layer.Domain = new DiscreteDomain<T>(filtered.ToDictionary(pair => pair.Key, pair => pair.Value, values.Comparer));
#pragma warning restore CS8714
            }

            return Math.Max(0, originalSize - SafeSize(layer.Domain));
        }

        private static int SafeSize(IDomain<T> domain)
        {
            int size;

            try
            {
                size = domain.Size();
            }
            catch (InternalLogicException)
            {
                return int.MaxValue;
            }

            return size;
        }

        #region Direct domains intersections

        private static IDomain<T> DiscreteDomainIntersection(DiscreteDomain<T> domain1, DiscreteDomain<T> domain2)
        {
#pragma warning disable CS8714
            Dictionary<T, int> intersection = domain1.Domain.Keys.Intersect(domain2.Domain.Keys).ToDictionary(x => x, x => domain1.Domain[x]);
#pragma warning restore CS8714

            if (intersection.Count == 0)
            {
                return EmptyDomain<T>.Instance;
            }

            return new DiscreteDomain<T>(intersection);
        }

        private static IDomain<T> NumericalDomainIntersection(NumericalDomain<T> domain1, NumericalDomain<T> domain2)
        {
            var result = new List<Interval<T>>();

            int i = 0, j = 0;
            while (i < domain1.Domain.Count && j < domain2.Domain.Count)
            {
                var interval1 = domain1.Domain[i];
                var interval2 = domain2.Domain[j];

                T start = NumericalDomain<T>.Max(interval1.Start, interval2.Start);
                T end = NumericalDomain<T>.Min(interval1.End, interval2.End);

                if (NumericalDomain<T>.Compare(start, end) <= 0)
                {
                    result.Add(new Interval<T>(start, end));
                }

                if (NumericalDomain<T>.Compare(interval1.End, interval2.End) < 0)
                    i++;
                else
                    j++;
            }

            if (result.Count == 0)
            {
                return EmptyDomain<T>.Instance;
            }

            return new NumericalDomain<T>(result);
        }

        private static IDomain<T> NumericalDiscreteDomainIntersection(NumericalDomain<T> domain1, DiscreteDomain<T> domain2)
        {
#pragma warning disable CS8714
            var result = new Dictionary<T, int>();
#pragma warning restore CS8714

            foreach (var key in domain2.Domain.Keys.Where(domain1.ContainsValue))
            {
                result[key] = 1;
            }

            if (result.Count == 0)
            {
                return EmptyDomain<T>.Instance;
            }

            return new DiscreteDomain<T>(result);
        }

        private static IDomain<T> DiscreteNumericalDomainIntersection(DiscreteDomain<T> domain1, NumericalDomain<T> domain2)
        {
#pragma warning disable CS8714
            var result = new Dictionary<T, int>();
#pragma warning restore CS8714

            foreach (var key in domain1.Domain.Keys.Where(domain2.ContainsValue))
            {
                result[key] = domain1.Domain[key];
            }

            if (result.Count == 0)
            {
                return EmptyDomain<T>.Instance;
            }

            return new DiscreteDomain<T>(result);
        }

        #endregion
    }
}
