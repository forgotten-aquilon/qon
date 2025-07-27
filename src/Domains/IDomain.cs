using System;
using System.Collections.Generic;

namespace qon.Domains
{
#pragma warning disable CS8714
    public interface IDomain<T>
    {
        int Size();
        bool IsEmpty();
        bool ContainsValue(T value);
        int Remove(T item);
        int Remove(List<T> items);
        void Clear();
        int Set(Dictionary<T, int> domain);
        double GetEntropy();
        void SetWeight(T value, int  weight);
        bool TryGetWeight(T value, out int weight);
        bool UpdateWeight(T value, int weight);
        T GetRandomValue(Random random);
        Optional<T> SingleOrEmptyValue();
        IDomain<T> Copy();
        IEnumerable<KeyValuePair<T, int>> GetIEnumerable();
    }
}
