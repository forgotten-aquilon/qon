using System;
using qon.Exceptions;
using qon.Functions;
using qon.Functions.Constraints;
using qon.Functions.Filters;
using qon.Layers;
using qon.Variables;

namespace qon.QSL
{
    public static partial class QSL
    {
        public static QSLConstraintBuilder<TQ> Constraint<TQ>() where TQ : notnull
        {
            return new QSLConstraintBuilder<TQ>();
        }

        public static QSLMutationBuilder<TQ> CreateMutation<TQ>() where TQ : notnull
        {
            return new QSLMutationBuilder<TQ>();
        }

        public static QSLMutationParameterBuilder<TQ> Mutation<TQ>() where TQ : notnull
        {
            return new QSLMutationParameterBuilder<TQ>();
        }

        public static Func<QVariable<TQ>, Result> VonNeumann<TQ>(EuclideanConstraintParameter<TQ> parameter) where TQ : notnull
        {
            ExceptionHelper.ThrowIfArgumentIsNull(parameter, nameof(parameter));

            return variable => VonNeumannFilter<TQ>.Filter.ApplyTo(variable)
                .Then(Functions.Propagators.Propagators.ToVonNeumann(parameter));
        }

        public static Func<QVariable<TQ>, Result> Moore<TQ>(EuclideanConstraintParameter<TQ> parameter) where TQ : notnull
        {
            ExceptionHelper.ThrowIfArgumentIsNull(parameter, nameof(parameter));

            return variable => MooreFilter<TQ>.Filter.ApplyTo(variable)
                .Then(Functions.Propagators.Propagators.ToMoore(parameter));
        }

        public static Func<QVariable<TQ>, QPredicate<TQ>> WithLayer<TQ, TLayer>(Func<TLayer, QPredicate<TQ>> predicate) where TQ : notnull
            where TLayer : ILayer<TQ, QVariable<TQ>>
        {
            return RelativeConstraint<TQ>.WithLayer(predicate);
        }
    }
}
