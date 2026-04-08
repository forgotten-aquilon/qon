using qon.Events;
using qon.Exceptions;
using qon.Functions;
using qon.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace qon.Variables.Domains
{
    public class DiscreteDomain<TQ> : IWeightedDomain<TQ> where TQ : notnull
    {
        private readonly Cache<double> _entropy;

        public Dictionary<TQ, int> Domain { get; protected set; }

        public DiscreteDomain()
        {
            Domain = new Dictionary<TQ, int>();
            _entropy = new Cache<double>(CalculateEntropy);
        }

        public DiscreteDomain(Dictionary<TQ, int> d)
        {
            Domain = d;
            _entropy = new Cache<double>(CalculateEntropy);
        }

        public DiscreteDomain(IEnumerable<TQ> d)
        {
            Domain = d.ToDictionary(x => x, _ => 1);
            _entropy = new Cache<double>(CalculateEntropy);
        }

        public DiscreteDomain(params TQ[] values)
        {
            Domain = values.ToDictionary(x => x, _ => 1);
            _entropy = new Cache<double>(CalculateEntropy);
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
        public bool ContainsValue(TQ value)
        {
            return Domain.ContainsKey(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Remove(TQ item)
        {
            var result = Domain.Remove(item) ? 1 : 0;

            if (result == 1)
            {
                _entropy.Changed();
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Remove(IEnumerable<TQ> items)
        {
            var changeCount = 0;

            foreach (var item in items) changeCount += Domain.Remove(item) ? 1 : 0;

            if (changeCount > 0)
            {
                _entropy.Changed();
            }

            return changeCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Domain.Clear();
            _entropy.Changed();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetEntropy()
        {
            return _entropy;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetWeight(TQ value, out int weight)
        {
            return Domain.TryGetValue(value, out weight);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool UpdateWeight(TQ value, int weight)
        {
            if (!Domain.ContainsKey(value)) return false;

            Domain[value] = weight;

            _entropy.Changed();

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TQ GetRandomValue(Random random)
        {
            var topProbabilityLimit = Domain.Sum(x => x.Value);

            if (topProbabilityLimit == 0 || IsEmpty())
                throw new InternalLogicException("Domain should have at least one non-zero weighted value");

            var probability = 0;

            var thresholdProbability = random.Next(1, topProbabilityLimit + 1);

            foreach (var probabilityItem in Domain)
            {
                //We ignore items with 0 weight
                if (probabilityItem.Value == 0)
                    continue;

                probability += probabilityItem.Value;

                if (probability >= thresholdProbability) return probabilityItem.Key;
            }

            throw new UnreachableException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Optional<TQ> SingleOrEmptyValue()
        {
            if (Domain.Count == 1) return Domain.First().Key;

            return Optional<TQ>.Empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TQ> GetValues()
        {
            return Domain.Select(x => x.Key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<KeyValuePair<TQ, int>> GetValuesWithWeights()
        {
            return Domain;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IDomain<TQ> Copy()
        {
            return new DiscreteDomain<TQ>(new Dictionary<TQ, int>(Domain));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            return string.Join(", ", Domain.Select(v => $"{v.Key}:{v.Value}"));
        }

        public bool Equals(IDomain<TQ> other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return other is DiscreteDomain<TQ> otherDomain && QSL.Helpers.DictionaryEquality(Domain, otherDomain.Domain);
        }

        private double CalculateEntropy()
        {
            var entropy = 0.0;
            double sum = Domain.Sum(x => x.Value);

            if (IsEmpty() || sum == 0)
                throw new InternalLogicException("Should not be called in this case");


            foreach (var value in Domain)
                if (value.Value != 0)
                {
                    var e = value.Value / sum;
                    entropy -= e * Math.Log(e, 2);
                }

            return entropy;
        }
    }
}