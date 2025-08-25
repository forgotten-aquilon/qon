using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qon.Domains
{
#pragma warning disable CS8714
   
    public class EmptyDomain<T> : IDomain<T>
    {
        public static EmptyDomain<T> Instance => Lazy.Value;

        private static readonly Lazy<EmptyDomain<T>> Lazy =
            new(() => new EmptyDomain<T>());

        private EmptyDomain()
        {
        }

        public int Size()
        {
            return 0;
        }

        public bool IsEmpty()
        {
            return true;
        }

        public bool ContainsValue(T value)
        {
            throw new InternalLogicException("Should never be called");
        }

        public int Remove(T item)
        {
            throw new InternalLogicException("Should never be called");
        }

        public int Remove(IEnumerable<T> items)
        {
            throw new InternalLogicException("Should never be called");
        }

        public void Clear()
        {
            throw new InternalLogicException("Should never be called");
        }

        public double GetEntropy()
        {
            throw new InternalLogicException("Should never be called");
        }

        public T GetRandomValue(Random random)
        {
            throw new InternalLogicException("Should never be called");
        }

        public Optional<T> SingleOrEmptyValue()
        {
            throw new InternalLogicException("Should never be called");
        }

        public IDomain<T> Copy()
        {
            return Instance;
        }

        public IEnumerable<KeyValuePair<T, int>> GetIEnumerable()
        {
            throw new InternalLogicException("Should never be called");
        }
    }
}
