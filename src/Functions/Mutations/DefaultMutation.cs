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
    public class DefaultMutation<TQ> : IMutationFunction<TQ> where TQ : notnull
    {
        private readonly IReplacer<TQ> _replacer;

        public DefaultMutation(IReplacer<TQ> replacer)
        {
            _replacer = replacer;
        }

        public List<Field<TQ>> ApplyTo(Field<TQ> input)
        {
            return _replacer.All(input);
        }
    }
}
