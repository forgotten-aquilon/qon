using System;
using System.Collections.Generic;
using System.Linq;
using qon.Variables;
using qon.Variables.Layers;

namespace qon.Functions.Propagators
{
    public class Propagator<T> : DefaultPropagator<IEnumerable<QVariable<T>>>
    {
        public Propagator(Func<IEnumerable<QVariable<T>>, ConstraintResult> propagationFunction) : base(propagationFunction)
        {
        }

        public override ConstraintResult ApplyTo(IEnumerable<QVariable<T>> input)
        {
            ConstraintResult result = PropagationFunction(input);

            if (result.Failed)
            {
                return result;
            }

            return input.Any(x => SuperpositionLayer<T>.With(x).State == SuperpositionState.Uncertain && SuperpositionLayer<T>.With(x).Domain.IsEmpty())
                ? ConstraintResult.HasErrors()
                : result;
        }

        public static IChain<IEnumerable<QVariable<T>>, ConstraintResult> operator ~(Propagator<T> obj)
        {
            return obj;
        }
    }
}
