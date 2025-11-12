using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Variables;

namespace qon.Functions
{
    public interface ISearcher<T>
    {
        public int SearchDepth { get; }
        public List<List<QVariable<T>>> Search(Field<T> field);
    }
}
