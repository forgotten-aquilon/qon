using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Variables;

namespace qon.Functions.Replacers
{
    public interface IReplacer<TQ> where TQ : notnull
    {
        public List<Field<TQ>> All(Field<TQ> field);
    }
}
