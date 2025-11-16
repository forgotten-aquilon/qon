using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Helpers;
using qon.Variables;

namespace qon.Functions.Mutations
{
    public class VariableMutation<TQ> where TQ : notnull
    {
        public Action<QVariable<TQ>> MutationFunction { get; protected set; } 

        public VariableMutation(Action<QVariable<TQ>> mutationFunction)
        {
            MutationFunction = mutationFunction;
        }

        public void Execute(QVariable<TQ> variable)
        {
            MutationFunction(variable);
        }
    }
}
