using System;
using System.Collections.Generic;
using qon.Helpers;

namespace qon.Variables.Domains
{
#pragma warning disable CS8714
    public interface IDomain<TQ> : ICopy<IDomain<TQ>> where TQ : notnull
    {
        int Size();
        bool IsEmpty();
        bool ContainsValue(TQ value);
        int Remove(TQ item);
        int Remove(IEnumerable<TQ> items);
        void Clear();
        double GetEntropy();
        TQ GetRandomValue(Random random);
        Optional<TQ> SingleOrEmptyValue();
        IEnumerable<TQ> GetValues();
    }
}
