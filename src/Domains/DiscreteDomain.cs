using System;
using System.Collections.Generic;
using System.Linq;

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

        public DiscreteDomain(List<T> d)
        {
            Domain = d.ToDictionary(x => x, _ => 1);
        }

        public int Size()
        {
            return Domain.Count;
        }

        public bool IsEmpty()
        {
            return Domain.Count == 0;
        }

        public bool ContainsValue(T value)
        {
            return Domain.ContainsKey(value);
        }

        public int Remove(T item)
        {
            return Domain.Remove(item) ? 1 : 0;
        }

        public int Remove(IEnumerable<T> items)
        {
            int changeCount = 0;

            foreach (var item in items)
            {
                changeCount += Domain.Remove(item) ? 1 : 0;
            }

            return changeCount;
        }

        public void Clear()
        {
            Domain.Clear();
        }

        public int Set(Dictionary<T, int> domain)
        {
            Domain = domain;

            return Domain.Count;
        }

        public double GetEntropy()
        {
            double entropy = 0.0;

            foreach (var domain in Domain)
            {
                double e = domain.Value / (double)Domain.Count;
                entropy -= e * Math.Log(e, 2);
            }

            return entropy;
        }

        public void SetWeight(T value, int weight)
        {
            Domain[value] = weight;
        }

        public bool TryGetWeight(T value, out int weight)
        {
            return Domain.TryGetValue(value, out weight);
        }

        public bool UpdateWeight(T value, int weight)
        {
            if (!Domain.ContainsKey(value))
            {
                return false;
            }

            Domain[value] = weight;
            return true;
        }

        public T GetRandomValue(Random random)
        {
            var topProbabilityLimit = Domain.Sum(x => x.Value);

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

            throw new InternalLogicException("");
        }

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
