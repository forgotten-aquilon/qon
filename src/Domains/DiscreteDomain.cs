using qon.Exceptions;
using qon.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace qon.Domains
{
    public class DiscreteDomain<T> : IDomain<T>
    {
#pragma warning disable CS8714
        public Dictionary<T, int> Domain { get; protected set; }

        public DiscreteDomain()
        {
            Domain = new Dictionary<T, int>();
        }

        public DiscreteDomain(Dictionary<T, int> d) 
        {
            Domain = d;
        }

        public DiscreteDomain(IEnumerable<T> d)
        {
            Domain = d.ToDictionary(x => x, _ => 1);
        }

        public DiscreteDomain(params T[] values)
        {
            Domain = values.ToDictionary(x => x, _ => 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Size()
        {
            return Domain.Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEmpty()
        {
            return Domain.Count == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsValue(T value)
        {
            return Domain.ContainsKey(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Remove(T item)
        {
            return Domain.Remove(item) ? 1 : 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Remove(IEnumerable<T> items)
        {
            int changeCount = 0;
            
            foreach (var item in items)
            {
                changeCount += Domain.Remove(item) ? 1 : 0;
            }

            return changeCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Domain.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Set(Dictionary<T, int> domain)
        {
            Domain = domain;

            return Domain.Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetEntropy()
        {

            double entropy = 0.0;
            double sum = Domain.Sum(x => x.Value);

            if (IsEmpty() || sum == 0)
                throw new InternalLogicException("Should not be called in this case");


            foreach (var value in Domain) if (value.Value != 0)
            {
                double e = value.Value / sum;
                entropy -= e * Math.Log(e, 2);
            }

            return entropy;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetWeight(T value, int weight)
        {
            Domain[value] = weight;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetWeight(T value, out int weight)
        {
            return Domain.TryGetValue(value, out weight);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool UpdateWeight(T value, int weight)
        {
            if (!Domain.ContainsKey(value))
            {
                return false;
            }

            Domain[value] = weight;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetRandomValue(Random random)
        {
            var topProbabilityLimit = Domain.Sum(x => x.Value);

            if (topProbabilityLimit == 0 || IsEmpty()) 
                throw new InternalLogicException("Domain should have at least one non-zero weighted value");

            int probability = 0;

            int thresholdProbability = random.Next(1, topProbabilityLimit + 1);

            foreach (var probabilityItem in Domain)
            {
                //We ignore items with 0 weight
                if (probabilityItem.Value == 0)
                    continue;

                probability += probabilityItem.Value;

                if (probability >= thresholdProbability)
                {
                    return probabilityItem.Key;
                }
            }

            throw new UnreachableException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Optional<T> SingleOrEmptyValue()
        {
            if (Domain.Count == 1)
            {
                return new Optional<T>(Domain.FirstOrDefault().Key);
            }

            return Optional<T>.Empty;
        }

        public IDomain<T> Copy()
        {
            return new DiscreteDomain<T>(new Dictionary<T, int>(Domain));
        }

        public IEnumerable<KeyValuePair<T, int>> GetIEnumerable()
        {
            return Domain;
        }

        public override string ToString()
        {
            return string.Join(", ", Domain.Select(v => $"{v.Key}:{v.Value}"));
        }
    }
}
