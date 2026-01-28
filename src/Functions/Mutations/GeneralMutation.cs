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
    public class GeneralMutationParameter<TQ> where TQ : notnull
    {
        public QPredicate<TQ> Filter { get; set; }
        public double Frequency { get; set; }
        public VariableMutation<TQ> MutationFunction { get; set; }

        public GeneralMutationParameter(QPredicate<TQ> filter, double frequency, VariableMutation<TQ> mutationFunction)
        {
            Filter = filter;
            Frequency = frequency;
            MutationFunction = mutationFunction;
        }
    }

    public class GeneralMutation<TQ> : IMutationFunction<TQ> where TQ : notnull
    {
        private readonly int _sampling = 1;

        private readonly List<GeneralMutationParameter<TQ>> _mutations;

        public GeneralMutation(List<GeneralMutationParameter<TQ>> mutation, int sampling)
        {
            _sampling = sampling;
            _mutations = mutation;
        }

        public List<Field<TQ>> ApplyTo(Field<TQ> field)
        {
            List<Field<TQ>> samples = new List<Field<TQ>>();

            for (int i = 0; i < _sampling; i++)
            {
                Field<TQ> sample = field.ShallowCopy();

                samples.Add(sample);
            }

            for (int i = 0; i < field.Count; i++)
            {
                var variable = field[i];

                foreach (var mutation in _mutations)
                {
                    if (mutation.Filter.ApplyTo(variable))
                    {
                        foreach (var sample in samples)
                        {
                            if (field.Machine.Random.GetRandomBool(mutation.Frequency))
                            {
                                var mutatedVariable = variable.Copy();
                                mutation.MutationFunction.Execute(mutatedVariable);

                                sample[i] = mutatedVariable;
                            }
                        }
                    }
                }
            }

            return samples;
        }
    }
}
