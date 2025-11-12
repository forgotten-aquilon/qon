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
    internal class DefaultMutation<T> : IPreparation<T>
    {
        private readonly IReplacer<T> _replacer;

        public DefaultMutation(IReplacer<T> replacer)
        {
            _replacer = replacer;
        }

        public Result Execute(Field<T> field, QMachine<T>? machine = null)
        {
            var samples = _replacer.All(field);

            if (samples.Count == 0)
            {
                return Result.HasErrors();
            }


            if (machine?.State is not null)
            {
                MutationLayer<T>.With(machine.State).Samples = samples;
            }

            return Result.Success(0);
        }
    }
}
