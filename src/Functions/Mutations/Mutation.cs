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
    public class Mutation<T> : IPreparation<T>
    {
        private QPredicate<T> _filter;
        private int _sampling = 1;
        private double _frequency = 1.0;
        private VariableMutation<T> _mutationFunction;
        public Mutation(QPredicate<T> filter, int sampling, double frequency, VariableMutation<T> implementation)
        {
            _filter = filter;
            _sampling = sampling;
            _frequency = frequency;
            _mutationFunction = implementation;
        }

        public Result Execute(QVariable<T>[] field, QMachine<T>? machine)
        {
            List<QVariable<T>[]> samples = new List<QVariable<T>[]>();

            for (int i = 0; i < _sampling; i++)
            {
                var sample = field.Select(x => x.Copy()).ToArray();

                foreach (var t in sample)
                {
                    if (_filter.ApplyTo(t) && (machine?.Random.GetRandomBool(_frequency) ?? false))
                    {
                        _mutationFunction.Execute(t);
                    }
                }

                samples.Add(sample);
            }

            if (machine?.State is not null)
            {
                MutationLayer<T>.With(machine.State).Samples = samples;
            }

            return Result.Success(0);
        }
    }
}
