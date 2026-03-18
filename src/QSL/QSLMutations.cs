using qon.Layers.StateLayers;
using qon.Machines;
using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
