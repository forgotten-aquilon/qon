using qon.Functions.Mutations;
using qon.Helpers;
using qon.Layers.StateLayers;
using qon.Layers.VariableLayers;
using qon.Machines;

namespace qon.QSL
{
    public static class Mutations
    {
        public static VariableMutation<TQ> RandomFromDomain<TQ>() where TQ : notnull
        {
            return new(v =>
            {
                v.Value = DomainLayer<TQ>.On(v).GetRandomValue(v.Machine.Random);
            });
        }

        public static VariableMutation<TQ> ToValue<TQ>(TQ value) where TQ : notnull
        {
            return new VariableMutation<TQ>(v =>
            {
                v.Value = value;
            });
        }

        public static QSLMutationBuilder<TQ> CreateMutation<TQ>() where TQ : notnull
        {
            return new QSLMutationBuilder<TQ>();
        }

        public static MutationParameterBuilder<TQ> Mutation<TQ>() where TQ : notnull
        {
            return new MutationParameterBuilder<TQ>();
        }

        public static QMachine<TQ> WithMutation<TQ>(this QMachine<TQ> machine, MutationLayerParameter<TQ> parameter) where TQ : notnull
        {
            MutationLayer<TQ>.GetOrCreate(machine.State).Parameter = parameter;

            return machine;
        }
    }
}
