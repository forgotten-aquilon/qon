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
        public static QSLConstraintBuilder<TQ> Constraint<TQ>() where TQ : notnull
        {
            return new QSLConstraintBuilder<TQ>();
        }

        public static QSLMutationBuilder<TQ> Mutation<TQ>() where TQ : notnull
        {
            return new QSLMutationBuilder<TQ>();
        }

        public static Func<QVariable<TQ>, Result> VonNeumann<TQ>(EuclideanConstraintParameter<TQ> parameter) where TQ : notnull
        {
            ExceptionHelper.ThrowIfArgumentIsNull(parameter, nameof(parameter));

            return variable =>
                new VonNeumannFilter<TQ>().ApplyTo(variable)
                + ~Propagators.Propagators.FromVonNeumann(parameter);
        }

        public static Func<QVariable<TQ>, QPredicate<TQ>> WithLayer<TQ, TLayer>(Func<TLayer, QPredicate<TQ>> predicate) where TQ : notnull
            where TLayer : ILayer<TQ, QVariable<TQ>>
        {
            return RelativeConstraint<TQ>.WithLayer(predicate);
        }
    }
}
