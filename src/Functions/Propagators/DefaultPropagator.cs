using System;
using System.Collections.Generic;
using System.Linq;
using qon.Variables;

namespace qon.Functions.Propagators
{
    public class DefaultPropagator<TIn> : IChain<TIn, Result>
    {
        public Func<TIn, Result> PropagationFunction { get; }

        public DefaultPropagator(Func<TIn, Result> propagationFunction)
        {
            PropagationFunction = propagationFunction;
        }

        public virtual Result ApplyTo(TIn input)
        {
            return PropagationFunction(input);
        }
    }
}