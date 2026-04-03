using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Helpers;
using qon.QSL;

namespace qon.Variables.Domains
{
    public class PrimitiveDomain<TQ> :IDomain<TQ> where TQ : notnull
    {
        public HashSet<TQ> Domain { get; protected set; }

        public PrimitiveDomain(HashSet<TQ> domain)
        {
            Domain = domain;
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
            return Domain.Remove(item) ? 1 : 0;
        }

        public int Remove(IEnumerable<TQ> items)
        {
            int preRemoveCount = Domain.Count;
            Domain.ExceptWith(items);
            int postRemoveCount = Domain.Count;

            return preRemoveCount - postRemoveCount;
        }

        public void Clear()
        {
            Domain.Clear();
        }

        public double GetEntropy()
        {
            return Math.Log(Size(), 2);
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

        #region Overrides of Object

        public override string ToString()
        {
            return string.Join(", ", Domain.Select(v => $"{v}"));
        }

        #endregion
    }
}
