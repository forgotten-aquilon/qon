using System;
using System.Collections.Generic;
using System.Linq;
using qon.Layers.VariableLayers;
using qon.Variables;

namespace qon.Functions.Propagators
{
    public class Propagator<TQ> : DefaultPropagator<IEnumerable<QVariable<TQ>>> where TQ : notnull
    {
        public Propagator(Func<IEnumerable<QVariable<TQ>>, Result> propagationFunction) : base(propagationFunction)
        {
        }

        public override Result ApplyTo(IEnumerable<QVariable<TQ>> input)
        {
            Result result = PropagationFunction(input);

            if (result.Failed)
            {
                return result;
            }

            return input.Any(x => x.State == ValueState.Uncertain && DomainLayer<TQ>.With(x).IsEmpty())
                ? Result.HasErrors()
                : result;
        }

        public static IChain<IEnumerable<QVariable<TQ>>, Result> operator ~(Propagator<TQ> obj)
        {
            return obj;
        }
    }
}
