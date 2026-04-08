using qon.Exceptions;
using qon.Helpers;
using qon.QSL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qon.Variables.Domains
{
    public class PrimitiveDomain<TQ> :IDomain<TQ> where TQ : notnull
    {
        private readonly Cache<double> _entropy;
        public HashSet<TQ> Domain { get; protected set; }

        public PrimitiveDomain(HashSet<TQ> domain)
        {
            Domain = domain;
            _entropy = new Cache<double>(CalculateEntropy);
        }

        #region IDomain<TQ>

        public int Size()
        {
            return Domain.Count;
        }

        public bool IsEmpty()
        {
            return Domain.Count == 0;
        }

        public bool ContainsValue(TQ value)
        {
            return Domain.Contains(value);
        }

        public int Remove(TQ item)
        {
            var result = Domain.Remove(item) ? 1 : 0;

            if (result == 1)
            {
                _entropy.Changed();
            }

            return result;
        }

        public int Remove(IEnumerable<TQ> items)
        {
            int preRemoveCount = Domain.Count;
            Domain.ExceptWith(items);
            int postRemoveCount = Domain.Count;

            int result = preRemoveCount - postRemoveCount;

            if (result > 0)
            {
                _entropy.Changed();
            }

            return result;
        }

        public void Clear()
        {
            Domain.Clear();
            _entropy.Changed();
        }

        public double GetEntropy()
        {
            return _entropy;
        }

        public TQ GetRandomValue(Random random)
        {
            return Domain.RandomItem(random);
        }

        public Optional<TQ> SingleOrEmptyValue()
        {
            if (Size() == 1)
            {
                return Domain.First();
            }

            return Optional<TQ>.Empty;
        }

        public IEnumerable<TQ> GetValues()
        {
            return Domain;
        }

        #endregion

        #region ICopy<IDomain<TQ>>

        public IDomain<TQ> Copy()
        {
            return new PrimitiveDomain<TQ>(new HashSet<TQ>(Domain));
        }

        #endregion

        public bool Equals(IDomain<TQ> other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return other is PrimitiveDomain<TQ> otherDomain && Domain.SetEquals(otherDomain.Domain);
        }

        #region Overrides of Object

        public override string ToString()
        {
            return string.Join(", ", Domain.Select(v => $"{v}"));
        }

        #endregion

        private double CalculateEntropy()
        {
            if (IsEmpty())
                throw new InternalLogicException("Should not be called in this case");

            return Math.Log(Size(), 2);
        }
    }
}
