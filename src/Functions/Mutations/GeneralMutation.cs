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
    public class GeneralMutation<T>
    {
        private readonly QPredicate<T> _filter;
        private readonly int _sampling = 1;
        private readonly double _frequency = 1.0;
        private readonly VariableMutation<T> _mutationFunction;

        public GeneralMutation(QPredicate<T> filter, int sampling, double frequency, VariableMutation<T> implementation)
        {
            _filter = filter;
            _sampling = sampling;
            _frequency = frequency;
            _mutationFunction = implementation;
        }

        public List<Field<T>> Execute(Field<T> field, QMachine<T>? machine = null)
        {
            List<Field<T>> samples = new List<Field<T>>();

            for (int i = 0; i < _sampling; i++)
            {
                Field<T> sample = field.Copy();

                foreach (var variable in sample)
                {
                    if (_filter.ApplyTo(variable) && field.Machine.Random.GetRandomBool(_frequency))
                    {
                        _mutationFunction.Execute(variable);
                    }
                }

                samples.Add(sample);
            } 

            return samples;
        }
    }
}
