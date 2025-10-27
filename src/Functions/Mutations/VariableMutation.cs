using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Helpers;
using qon.Variables;

namespace qon.Functions.Mutations
{
    public class VariableMutation<T>
    {
        public Action<QVariable<T>> MutationFunction { get; protected set; }

        public VariableMutation(Action<QVariable<T>> mutationFunction)
        {
            MutationFunction = mutationFunction;
        }

        public void Execute(QVariable<T> variable)
        {
            MutationFunction(variable);
        }
    }
}
