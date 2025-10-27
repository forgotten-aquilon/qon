using System;
using qon.Exceptions;
using qon.Functions.Constraints;
using qon.Functions.Filters;
using qon.Layers;
using qon.Variables;

namespace qon.Functions.QSL
{
    public static class QSL
    {
        public static QSLConstraintBuilder<T> Constraint<T>()
        {
            return new QSLConstraintBuilder<T>();
        }

        public static QSLMutationBuilder<T> Mutation<T>()
        {
            return new QSLMutationBuilder<T>();
        }

        public static Func<QVariable<T>, Result> VonNeumann<T>(EuclideanConstraintParameter<T> parameter)
        {
            ExceptionHelper.ThrowIfArgumentIsNull(parameter, nameof(parameter));

            return variable =>
                new VonNeumannFilter<T>().ApplyTo(variable)
                + ~Propagators.Propagators.FromVonNeumann(parameter);
        }

        public static Func<QVariable<T>, QPredicate<T>> WithLayer<T, TLayer>(Func<TLayer, QPredicate<T>> predicate)
            where TLayer : ILayer<T, QVariable<T>>
        {
            return RelativeConstraint<T>.WithLayer(predicate);
        }
    }
}
