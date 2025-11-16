using System.Collections.Generic;

namespace qon.Variables.Domains
{
    public interface IWeightedDomain<TQ> : IDomain<TQ> where TQ : notnull
    {
        IEnumerable<KeyValuePair<TQ, int>> GetValuesWithWeights();
        bool UpdateWeight(TQ value, int weight);
    }
}
