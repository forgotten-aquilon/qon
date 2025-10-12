using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qon.Functions.Operations
{
    public class Operation<TIn, TOut> : IChain<TIn, TOut>
    {
        public Func<TIn, TOut> OperationFunction { get; }

        public Operation(Func<TIn, TOut> operationFunction)
        {
            OperationFunction = operationFunction;
        }

        public TOut ApplyTo(TIn input)
        {
            throw new NotImplementedException();
        }

        public IChain<TIn, TOut> AsIChain()
        {
            throw new NotImplementedException();
        }
    }
}
