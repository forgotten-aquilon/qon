using qon.Functions.Mutations;
using qon.Layers.StateLayers;
using qon.Machines;
using System;
using System.Collections.Generic;
using System.Text;
using qon.Helpers;
using qon.Layers.VariableLayers;

namespace qon
{
    public static partial class QSL
    {
        public static QSLMutationBuilder<TQ> CreateMutation<TQ>() where TQ : notnull
        {
            return new QSLMutationBuilder<TQ>();
        }

        public static QSLMutationParameterBuilder<TQ> Mutation<TQ>() where TQ : notnull
        {
            return new QSLMutationParameterBuilder<TQ>();
        }

        public static QMachine<TQ> WithMutation<TQ>(this QMachine<TQ> machine, MutationLayerParameter<TQ> parameter) where TQ : notnull
        {
            MutationLayer<TQ>.GetOrCreate(machine.State).Parameter = parameter;

            return machine;
        }

        public static class Mutations<TQ> where TQ : notnull
        {
            public static VariableMutation<TQ> RandomFromDomain = new(v =>
            {
                v.Value = Optional<TQ>.Of(DomainLayer<TQ>.With(v).GetRandomValue(v.Machine.Random));
            });

            public static VariableMutation<TQ> ToValue(TQ value)
            {
                return new VariableMutation<TQ>(v =>
                {
                    v.Value = Optional<TQ>.Of(value);
                });
            }
        }
    }
}
