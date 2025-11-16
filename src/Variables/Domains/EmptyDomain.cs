using System;
using System.Collections.Generic;
using qon.Exceptions;
using qon.Helpers;

namespace qon.Variables.Domains
{
    public class EmptyDomain<TQ> : IDomain<TQ> where TQ : notnull
    {
        public static EmptyDomain<TQ> Instance => Lazy.Value;

        private static readonly Lazy<EmptyDomain<TQ>> Lazy =
            new(() => new EmptyDomain<TQ>());

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

        public bool ContainsValue(TQ value)
        {
            throw new InternalLogicException("Should never be called");
        }

        public int Remove(TQ item)
        {
            throw new InternalLogicException("Should never be called");
        }

        public int Remove(IEnumerable<TQ> items)
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

        public TQ GetRandomValue(Random random)
        {
            throw new InternalLogicException("Should never be called");
        }

        public Optional<TQ> SingleOrEmptyValue()
        {
            throw new InternalLogicException("Should never be called");
        }

        public IDomain<TQ> Copy()
        {
            return Instance;
        }

        public IEnumerable<TQ> GetValues()
        {
            throw new InternalLogicException("Should never be called");
        }
    }
}