using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Variables;

namespace qon.Functions.Replacers
{
    public interface IReplacer<T>
    {
        public List<Field<T>> All(Field<T> field);
    }
}
