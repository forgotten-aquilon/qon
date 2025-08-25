using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace qon.Domains
{
    public static class DomainHelper<T>
    {
        private static readonly Dictionary<(Type, Type), Func<IDomain<T>, IDomain<T>, IDomain<T>>> Map = new();

        public static void Register<T1, T2, TOut>(Func<T1, T2, TOut> handler) where T1: IDomain<T> where T2: IDomain<T> where TOut: IDomain<T>
            => Map[(typeof(T1), typeof(T2))] = (a, b) => handler((T1)a, (T2)b);

        public static bool TryGet(IDomain<T> a, IDomain<T> b, out Func<IDomain<T>, IDomain<T>, IDomain<T>>? func)
            => Map.TryGetValue((a.GetType(), b.GetType()), out func);

        static DomainHelper()
        {
            Register<DiscreteDomain<T>, DiscreteDomain<T>, IDomain<T>>(DiscreteDomainIntersection);
            Register<NumericalDomain<T>, NumericalDomain<T>, IDomain<T>>(NumericalDomainIntersection);
            Register<DiscreteDomain<T>, NumericalDomain<T>, IDomain<T>>(DiscreteNumericalDomainIntersection);
            Register<NumericalDomain<T>, DiscreteDomain<T>, IDomain<T>>(NumericalDiscreteDomainIntersection);
        }

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
    }
}
