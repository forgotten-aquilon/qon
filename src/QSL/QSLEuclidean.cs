using qon.Exceptions;
using qon.Functions;
using qon.Functions.Propagators;
using qon.Functions.Constraints;
using qon.Functions.Filters;
using qon.Layers;
using qon.Layers.StateLayers;
using qon.Machines;
using qon.Variables;
using System;
using qon.Layers.VariableLayers;

namespace qon
{
    public static partial class QSL
    {
        public static Func<QObject<TQ>, Result> VonNeumann<TQ>(EuclideanConstraintParameter<TQ> parameter) where TQ : notnull
        {
            ExceptionHelper.ThrowIfArgumentIsNull(parameter, nameof(parameter));

            return variable => VonNeumannFilter<TQ>.CreateParameter(variable)
                .Then(Propagators.ToVonNeumann(parameter));
        }

        public static Func<QObject<TQ>, Result> Moore<TQ>(EuclideanConstraintParameter<TQ> parameter) where TQ : notnull
        {
            ExceptionHelper.ThrowIfArgumentIsNull(parameter, nameof(parameter));

            //TODO: Propagators.Propagators????
            return variable => MooreFilter<TQ>.CreateParameter(variable)
                .Then(Propagators.ToMoore(parameter));
        }

        public static Func<QObject<TQ>, QPredicate<TQ>> OnLayer<TQ, TLayer>(Func<TLayer, QPredicate<TQ>> predicate) where TQ : notnull
            where TLayer : ILayer<TQ, QObject<TQ>>
        {
            return RelativeConstraint<TQ>.WithLayer(predicate);
        }

        public static Func<QObject<TQ>, QPredicate<TQ>> Euclidean<TQ>(Func<EuclideanLayer<TQ>, EuclideanLayer<TQ>, bool> func) where TQ : notnull
        {
            return variable =>
            {
                var flayer = EuclideanLayer<TQ>.With(variable);

                bool Aggregation(QObject<TQ> newObject)
                {
                    var slayer = EuclideanLayer<TQ>.With(newObject);

                    return func(flayer, slayer);
                }

                return new QPredicate<TQ>(Aggregation);
            };
        }

        public static QObject<TQ> At<TQ>(this QMachine<TQ> machine, int x, int y, int z) where TQ : notnull
        {
            return ExceptionHelper.ThrowIfInternalValueIsNull(EuclideanStateLayer<TQ>.With(machine.State)[x, y, z]);
        }
    }
}
