using System.Collections.Generic;

namespace qon.Variables.Domains
{
    public interface IWeightedDomain<T> : IDomain<T>
    {
        IEnumerable<KeyValuePair<T, int>> GetValuesWithWeights();
    }
}
