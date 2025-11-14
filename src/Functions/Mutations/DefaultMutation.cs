using qon.Functions.Replacers;
using qon.Layers.StateLayers;
using qon.Machines;
using qon.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qon.Functions.Mutations
{
    public class DefaultMutation<T>
    {
        private readonly IReplacer<T> _replacer;

        public DefaultMutation(IReplacer<T> replacer)
        {
            _replacer = replacer;
        }

        public List<Field<T>> Execute(Field<T> field)
        {
            var samples = _replacer.All(field);

            return samples;
        }
    }
}
