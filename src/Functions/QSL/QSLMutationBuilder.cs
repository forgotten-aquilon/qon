using qon.Exceptions;
using qon.Functions.Filters;
using qon.Functions.Mutations;
using qon.Variables;
using System;
using System.Collections.Generic;
using qon.Machines;

namespace qon.Functions.QSL
{
    public class QSLMutationParameterBuilder<TQ> where TQ : notnull
    {
        private QPredicate<TQ>? _guard;
        private double _frequency = 1.0;
        private VariableMutation<TQ>? _mutation;

        public QSLMutationParameterBuilder<TQ> When(QPredicate<TQ> guard)
        {
            _guard = guard;
            return this;
        }

        public QSLMutationParameterBuilder<TQ> Frequency(double frequency)
        {
            _frequency = frequency;
            return this;
        }

        public QSLMutationParameterBuilder<TQ> Into(VariableMutation<TQ> mutation)
        {
            _mutation = mutation;
            return this;
        }

        public QSLMutationParameterBuilder<TQ> Into(Action<QVariable<TQ>> mutationFunction)
        {
            _mutation = new VariableMutation<TQ>(mutationFunction);
            return this;
        }

        public GeneralMutationParameter<TQ> Build()
        {
            ExceptionHelper.ThrowIfInternalValueIsNull(_guard, nameof(_guard));
            ExceptionHelper.ThrowIfInternalValueIsNull(_mutation, nameof(_mutation));
            return new GeneralMutationParameter<TQ>(_guard, _frequency, _mutation);
        }
    }

    public class QSLMutationBuilder<TQ> where TQ : notnull
    {
        private int _sampling = 1;
        private List<GeneralMutationParameter<TQ>> _mutations = new List<GeneralMutationParameter<TQ>>();

        public QSLMutationBuilder<TQ> Sampling(int sampling)
        {
            _sampling = sampling;
            return this;
        }

        public QSLMutationBuilder<TQ> AddMutation(GeneralMutationParameter<TQ> mutation)
        {
            _mutations.Add(mutation);
            return this;
        }

        public Func<Field<TQ>, List<Field<TQ>>> Build()
        {
            var mutation = new GeneralMutation<TQ>(_mutations, _sampling);

            return field => mutation.Execute(field);
        }
    }
}