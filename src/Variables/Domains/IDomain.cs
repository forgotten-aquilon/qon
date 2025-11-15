using System;
using System.Collections.Generic;
using qon.Helpers;

namespace qon.Variables.Domains
{
#pragma warning disable CS8714
    public interface IDomain<T> : ICopy<IDomain<T>>
    {
        int Size();
        bool IsEmpty();
        bool ContainsValue(T value);
        int Remove(T item);
        int Remove(IEnumerable<T> items);
        void Clear();
        double GetEntropy();
        T GetRandomValue(Random random);
        Optional<T> SingleOrEmptyValue();
        IEnumerable<T> GetValues();
    }
}
