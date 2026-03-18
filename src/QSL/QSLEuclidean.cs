using qon.Exceptions;
using qon.Functions;
using qon.Functions.Constraints;
using qon.Functions.Filters;
using qon.Layers;
using qon.Layers.StateLayers;
using qon.Machines;
using qon.Variables;
using System;

namespace qon
{
    public static partial class QSL
    {
        public static Func<QVariable<TQ>, Result> VonNeumann<TQ>(EuclideanConstraintParameter<TQ> parameter) where TQ : notnull
        {
            ExceptionHelper.ThrowIfArgumentIsNull(parameter, nameof(parameter));

            return variable => VonNeumannFilter<TQ>.CreateParameter(variable)
                .Then(Functions.Propagators.Propagators.ToVonNeumann(parameter));
        }

        public static Func<QVariable<TQ>, Result> Moore<TQ>(EuclideanConstraintParameter<TQ> parameter) where TQ : notnull
        {
            ExceptionHelper.ThrowIfArgumentIsNull(parameter, nameof(parameter));

            //TODO: Propagators.Propagators????
            return variable => MooreFilter<TQ>.CreateParameter(variable)
                .Then(Functions.Propagators.Propagators.ToMoore(parameter));
        }

        public static Func<QVariable<TQ>, QPredicate<TQ>> WithLayer<TQ, TLayer>(Func<TLayer, QPredicate<TQ>> predicate) where TQ : notnull
            where TLayer : ILayer<TQ, QVariable<TQ>>
        {
            return RelativeConstraint<TQ>.WithLayer(predicate);
        }

        public static QVariable<TQ> At<TQ>(this QMachine<TQ> machine, int x, int y, int z) where TQ : notnull
        {
            return ExceptionHelper.ThrowIfInternalValueIsNull(EuclideanStateLayer<TQ>.With(machine.State)[x, y, z]);
        }
    }
}
