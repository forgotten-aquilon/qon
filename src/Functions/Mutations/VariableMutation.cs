using qon.Helpers;
using qon.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace qon.Functions.Mutations
{
    public class VariableMutation<TQ> where TQ : notnull
    {
        public static VariableMutation<TQ> Empty => new VariableMutation<TQ>(new Action<QObject<TQ>>(_ => { }));

        public Action<QObject<TQ>> MutationFunction { get; protected set; } 

        public VariableMutation(Action<QObject<TQ>> mutationFunction)
        {
            MutationFunction = mutationFunction;
        }

        public void Execute(QObject<TQ> @object)
        {
            MutationFunction(@object);
        }

        public static VariableMutation<TQ> FromValue(TQ value)
        {
            return new VariableMutation<TQ>(v => v.Value = Optional<TQ>.Of(value));
        }
    }
}
