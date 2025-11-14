using qon.Exceptions;
using qon.Functions.Filters;
using qon.Functions.Mutations;
using qon.Variables;
using System;
using System.Collections.Generic;

namespace qon.Functions.QSL
{
    public class QSLMutationBuilder<T>
    {
        private QPredicate<T>? _guard;
        private double _frequency = 1.0;
        private int _sampling = 1;
        private VariableMutation<T>? _mutation;

        public QSLMutationBuilder<T> When(QPredicate<T> guard)
        {
            _guard = guard;
            return this;
        }

        public QSLMutationBuilder<T> Frequency(double frequency)
        {
            _frequency = frequency;
            return this;
        }

        public QSLMutationBuilder<T> Sampling(int sampling)
        {
            _sampling = sampling;
            return this;
        }

        public QSLMutationBuilder<T> Into(VariableMutation<T> mutation)
        {
            _mutation = mutation;
            return this;
        }

        public QSLMutationBuilder<T> Into(Action<QVariable<T>> mutationFunction)
        {
            _mutation = new VariableMutation<T>(mutationFunction);
            return this;
        }


        public Func<Field<T>, List<Field<T>>> Build()
        {
            ExceptionHelper.ThrowIfInternalValueIsNull(_guard, nameof(_guard));
            ExceptionHelper.ThrowIfInternalValueIsNull(_mutation, nameof(_mutation));

            var mutation = new GeneralMutation<T>(_guard, _sampling, _frequency, _mutation);

            return field => mutation.Execute(field);
        }
    }
}