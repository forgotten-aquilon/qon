using System;
using System.Collections.Generic;
using System.Linq;
using qon.Layers.VariableLayers;
using qon.Variables;

namespace qon.Functions.Propagators
{
    public class Propagator<T> : DefaultPropagator<IEnumerable<QVariable<T>>>
    {
        public Propagator(Func<IEnumerable<QVariable<T>>, Result> propagationFunction) : base(propagationFunction)
        {
        }

        public override Result ApplyTo(IEnumerable<QVariable<T>> input)
        {
            Result result = PropagationFunction(input);

            if (result.Failed)
            {
                return result;
            }

            return input.Any(x => x.State == ValueState.Uncertain && DomainLayer<T>.With(x).IsEmpty())
                ? Result.HasErrors()
                : result;
        }

        public static IChain<IEnumerable<QVariable<T>>, Result> operator ~(Propagator<T> obj)
        {
            return obj;
        }
    }
}
