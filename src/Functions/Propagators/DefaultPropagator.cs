using System;
using System.Collections.Generic;
using System.Linq;
using qon.Variables;

namespace qon.Functions.Propagators
{
    public class DefaultPropagator<TIn> : IChain<TIn, ConstraintResult>
    {
        public Func<TIn, ConstraintResult> PropagationFunction { get; }

        public DefaultPropagator(Func<TIn, ConstraintResult> propagationFunction)
        {
            PropagationFunction = propagationFunction;
        }

        public virtual ConstraintResult ApplyTo(TIn input)
        {
            return PropagationFunction(input);
        }

        public static IChain<TIn, ConstraintResult> operator ~(DefaultPropagator<TIn> obj)
        {
            return obj;
        }
    }
}