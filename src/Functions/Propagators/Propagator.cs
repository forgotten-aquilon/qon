using System;
using System.Collections.Generic;
using System.Linq;
using qon.Variables;

namespace qon.Functions.Propagators
{
    public class Propagator<T> : DefaultPropagator<IEnumerable<SuperpositionVariable<T>>>
    {
        public Propagator(Func<IEnumerable<SuperpositionVariable<T>>, ConstraintResult> propagationFunction) : base(propagationFunction)
        {
        }

        public override ConstraintResult ApplyTo(IEnumerable<SuperpositionVariable<T>> input)
        {
            ConstraintResult result = PropagationFunction(input);

            return result.Failed switch
            {
                true => result,
                false => input.Any(x => x.State == SuperpositionState.Uncertain && x.Domain.IsEmpty()) 
                    ? ConstraintResult.HasErrors()
                    : result
            };
        }

        public static IChain<IEnumerable<SuperpositionVariable<T>>, ConstraintResult> operator ~(Propagator<T> obj)
        {
            return obj;
        }
    }
}