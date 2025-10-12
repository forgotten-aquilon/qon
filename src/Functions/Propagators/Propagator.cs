using System;
using System.Collections.Generic;
using System.Linq;
using qon.Variables;

namespace qon.Functions.Propagators
{
    public class Propagator<T> : DefaultPropagator<T, IEnumerable<SuperpositionVariable<T>>>
    {
        public Propagator(Func<IEnumerable<SuperpositionVariable<T>>, ConstraintResult> propagationFunction) : base(propagationFunction)
        {
        }

        public override ConstraintResult ApplyTo(IEnumerable<SuperpositionVariable<T>> input)
        {
            ConstraintResult result = PropagationFunction(input);

            return result.IsSuccess switch
            {
                false => result,
                true => input.Any(x => x.State == SuperpositionState.Uncertain && x.Domain.IsEmpty()) 
                    ? new ConstraintResult(result.IsSuccess, result.ChangesAmount)
                    : result
            };
        }

        public override IChain<IEnumerable<SuperpositionVariable<T>>, ConstraintResult> AsIChain()
        {
            return this;
        }
    }
}