using System;
using System.Collections.Generic;
using qon.Exceptions;
using qon.Functions.Filters;
using qon.Functions.Mutations;
using qon.Machines;
using qon.Variables;

namespace qon.QSL
{
    public class MutationParameterBuilder<TQ> where TQ : notnull
    {
        private QPredicate<TQ>? _guard;
        private double _frequency = 1.0;
        private VariableMutation<TQ>? _mutation;
        private Func<Field<TQ>, bool>? _fieldCheck;

        public MutationParameterBuilder<TQ> When(QPredicate<TQ> guard)
        {
            _guard = guard;
            return this;
        }

        public MutationParameterBuilder<TQ> WhenField(Func<Field<TQ>, bool> condition)
        {
            _fieldCheck = condition;
            return this;
        }

        public MutationParameterBuilder<TQ> Frequency(double frequency)
        {
            _frequency = frequency;
            return this;
        }

        public MutationParameterBuilder<TQ> Into(VariableMutation<TQ> mutation)
        {
            _mutation = mutation;
            return this;
        }

        public MutationParameterBuilder<TQ> Into(TQ mutationValue)
        {
            _mutation = VariableMutation<TQ>.FromValue(mutationValue);
            return this;
        }

        public MutationParameterBuilder<TQ> Into(Action<QObject<TQ>> mutationFunction)
        {
            _mutation = new VariableMutation<TQ>(mutationFunction);
            return this;
        }

        public GeneralMutationParameter<TQ> Build()
        {
            ExceptionHelper.ThrowIfInternalValueIsNull(_guard, nameof(_guard));
            ExceptionHelper.ThrowIfInternalValueIsNull(_mutation, nameof(_mutation));
            return new GeneralMutationParameter<TQ>(_guard, _frequency, _mutation, _fieldCheck);
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

        public MutationFunction<TQ> Build()
        {
            return new GeneralMutation<TQ>(_mutations, _sampling);
        }
    }
}