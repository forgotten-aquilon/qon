using System.Collections.Generic;
using qon.Machines;
using qon.Variables;

namespace qon.Functions.Searchers
{
    public interface ISearcher<TQ> where TQ : notnull
    {
        public int SearchDepth { get; }
        public List<List<QVariable<TQ>>> Search(Field<TQ> field);
    }
}
