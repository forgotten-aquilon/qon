using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Helpers;

namespace qon.Variables.Domains
{
    public class PrimitiveDomain<T> :IDomain<T>
    {
        public HashSet<T> Domain { get; protected set; }

        public PrimitiveDomain(HashSet<T> domain)
        {
            Domain = domain;
        }

        #region IDomain<T>

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
            return Domain.Contains(value);
        }

        public int Remove(T item)
        {
            return Domain.Remove(item) ? 1 : 0;
        }

        public int Remove(IEnumerable<T> items)
        {
            int preRemoveCount = Domain.Count;
            Domain.ExceptWith(items);
            int postRemoveCount = Domain.Count;

            return postRemoveCount - preRemoveCount;
        }

        public void Clear()
        {
            Domain.Clear();
        }

        public double GetEntropy()
        {
            return Math.Log(Size(), 2);
        }

        public T GetRandomValue(Random random)
        {
            return Domain.RandomItem(random);
        }

        public Optional<T> SingleOrEmptyValue()
        {
            if (Size() == 1)
            {
                return Optional<T>.Of(Domain.First());
            }

            return Optional<T>.Empty;
        }

        public IEnumerable<T> GetValues()
        {
            return Domain;
        }

        #endregion

        #region ICopy<IDomain<T>>

        public IDomain<T> Copy()
        {
            return new PrimitiveDomain<T>(new HashSet<T>(Domain));
        }

        #endregion
    }
}
