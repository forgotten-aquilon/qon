using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Functions.Filters;
using qon.Helpers;
using qon.Layers.StateLayers;
using qon.Machines;
using qon.Variables;

namespace qon.Functions.Mutations
{
    public class GeneralMutation<TQ> where TQ : notnull
    {
        private readonly QPredicate<TQ> _filter;
        private readonly int _sampling = 1;
        private readonly double _frequency = 1.0;
        private readonly VariableMutation<TQ> _mutationFunction;

        public GeneralMutation(QPredicate<TQ> filter, int sampling, double frequency, VariableMutation<TQ> implementation)
        {
            _filter = filter;
            _sampling = sampling;
            _frequency = frequency;
            _mutationFunction = implementation;
        }

        public List<Field<TQ>> Execute(Field<TQ> field)
        {
            List<Field<TQ>> samples = new List<Field<TQ>>();

            for (int i = 0; i < _sampling; i++)
            {
                QVariable<TQ>[] fieldCopy = new QVariable<TQ>[field.Count];
                Array.Copy(field.Variables, fieldCopy, field.Count);
                Field<TQ> sample = new Field<TQ>(field.Machine, fieldCopy);

                samples.Add(sample);
            }

            for (int i = 0; i < field.Count; i++)
            {
                var variable = field[i];

                if (_filter.ApplyTo(variable))
                {
                    foreach (var sample in samples)
                    {
                        if (field.Machine.Random.GetRandomBool(_frequency))
                        {
                            var mutatedVariable = variable.Copy();
                            _mutationFunction.Execute(mutatedVariable);

                            sample[i] = mutatedVariable;
                        }
                    }
                }
            }

            return samples;
        }
    }
}
