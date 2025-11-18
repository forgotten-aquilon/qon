using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Machines;
using qon.Variables;

namespace qon.Functions
{
    public interface ISearcher<TQ> where TQ : notnull
    {
        public int SearchDepth { get; }
        public List<List<QVariable<TQ>>> Search(Field<TQ> field);
    }
}
