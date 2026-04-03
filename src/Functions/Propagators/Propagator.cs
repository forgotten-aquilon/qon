using System;
using System.Collections.Generic;
using System.Linq;
using qon.Layers.VariableLayers;
using qon.QSL;
using qon.Variables;

namespace qon.Functions.Propagators
{
    public class Propagator<TQ> : DefaultPropagator<IEnumerable<QObject<TQ>>> where TQ : notnull
    {
        public Propagator(Func<IEnumerable<QObject<TQ>>, Result> propagationFunction) : base(propagationFunction)
        {
        }

        public override Result ApplyTo(IEnumerable<QObject<TQ>> input)
        {
            Result result = PropagationFunction(input);

            if (result.Failed)
            {
                return result;
            }

            return input.Any(x => x.OnDomainLayer().State == ValueState.Uncertain && x.OnDomainLayer().IsEmpty())
                ? Result.HasErrors()
                : result;
        }

        public static IChain<IEnumerable<QObject<TQ>>, Result> operator ~(Propagator<TQ> obj)
        {
            return obj;
        }
    }
}
