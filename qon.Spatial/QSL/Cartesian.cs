using qon.Exceptions;
using qon.Functions;
using qon.Functions.Constraints;
using qon.Functions.Filters;
using qon.Functions.Propagators;
using qon.Layers;
using qon.Layers.StateLayers;
using qon.Layers.VariableLayers;
using qon.Machines;
using qon.Variables;
using System;
using System.Runtime.CompilerServices;

namespace qon.QSL
{
    public static partial class Cartesian
    {
        public static Func<QObject<TQ>, Result> VonNeumann<TQ>(CartesianConstraintParameter<TQ> parameter) where TQ : notnull
        {
            ExceptionHelper.ThrowIfArgumentIsNull(parameter, nameof(parameter));

            return variable => VonNeumannFilter<TQ>.CreateParameter(variable)
                .Then(CartesianPropagators.ToVonNeumann(parameter));
        }

        public static Func<QObject<TQ>, Result> Moore<TQ>(CartesianConstraintParameter<TQ> parameter) where TQ : notnull
        {
            ExceptionHelper.ThrowIfArgumentIsNull(parameter, nameof(parameter));

            return variable => MooreFilter<TQ>.CreateParameter(variable)
                .Then(CartesianPropagators.ToMoore(parameter));
        }

        public static Func<QObject<TQ>, QPredicate<TQ>> OnLayer<TQ, TLayer>(Func<TLayer, QPredicate<TQ>> predicate) where TQ : notnull
            where TLayer : ILayer<TQ, QObject<TQ>>
        {
            return RelativeConstraint<TQ>.WithLayer(predicate);
        }

        public static Func<QObject<TQ>, QPredicate<TQ>> OnLayer<TQ>(Func<CartesianLayer<TQ>, CartesianLayer<TQ>, bool> func) where TQ : notnull
        {
            return variable =>
            {
                var flayer = CartesianLayer<TQ>.On(variable);

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                bool Aggregation(QObject<TQ> newObject)
                {
                    var slayer = CartesianLayer<TQ>.On(newObject);

                    return func(flayer, slayer);
                }

                return new QPredicate<TQ>(Aggregation);
            };
        }

        public static QObject<TQ> At<TQ>(this QMachine<TQ> machine, int x, int y, int z) where TQ : notnull
        {
            return ExceptionHelper.ThrowIfInternalValueIsNull(CartesianStateLayer<TQ>.On(machine.State)[x, y, z]);
        }
    }
}
