using qon.Exceptions;
using qon.Functions.Filters;
using qon.Functions.Mutations;
using qon.Variables;
using System;
using System.Collections.Generic;

namespace qon.Functions.QSL
{
    public class QSLMutationBuilder<TQ> where TQ : notnull
    {
        private QPredicate<TQ>? _guard;
        private double _frequency = 1.0;
        private int _sampling = 1;
        private VariableMutation<TQ>? _mutation;

        public QSLMutationBuilder<TQ> When(QPredicate<TQ> guard)
        {
            _guard = guard;
            return this;
        }

        public QSLMutationBuilder<TQ> Frequency(double frequency)
        {
            _frequency = frequency;
            return this;
        }

        public QSLMutationBuilder<TQ> Sampling(int sampling)
        {
            _sampling = sampling;
            return this;
        }

        public QSLMutationBuilder<TQ> Into(VariableMutation<TQ> mutation)
        {
            _mutation = mutation;
            return this;
        }

        public QSLMutationBuilder<TQ> Into(Action<QVariable<TQ>> mutationFunction)
        {
            _mutation = new VariableMutation<TQ>(mutationFunction);
            return this;
        }


        public Func<Field<TQ>, List<Field<TQ>>> Build()
        {
            ExceptionHelper.ThrowIfInternalValueIsNull(_guard, nameof(_guard));
            ExceptionHelper.ThrowIfInternalValueIsNull(_mutation, nameof(_mutation));

            var mutation = new GeneralMutation<TQ>(_guard, _sampling, _frequency, _mutation);

            return field => mutation.Execute(field);
        }
    }
}