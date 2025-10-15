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
            return OperationFunction(input);
        }

        public static IChain<TIn, TOut> operator ~(Operation<TIn, TOut> obj)
        {
            return obj;
        }
    }
}
