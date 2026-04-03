using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Helpers;
using qon.Variables;

namespace qon.Functions.Mutations
{
    public static class Mutators
    {
        public class DefaultMutator<TQ> : IMutator<TQ> where TQ : notnull
        {
            private readonly List<VariableMutation<TQ>> _mutations;
            public int MutationCount => _mutations.Count;

            public DefaultMutator(List<VariableMutation<TQ>> mutations)
            {
                _mutations = mutations;
            }

            public void Mutate(List<QObject<TQ>> variables)
            {
                for (int i = 0; i < variables.Count; i++)
                {
                    _mutations[i].Execute(variables[i]);
                }
            }
        }


        public class ValueMutator<TQ> : IMutator<TQ> where TQ : notnull
        {
            private readonly List<TQ> _possibleValues;
            public int MutationCount => _possibleValues.Count;

            public ValueMutator(IEnumerable<TQ> possibleValues)
            {
                _possibleValues = possibleValues.ToList();
            }

            public ValueMutator(params TQ[] possibleValues)
            {
                _possibleValues = possibleValues.ToList();
            }

            public void Mutate(List<QObject<TQ>> variables)
            {
                for (int i = 0; i < variables.Count; i++)
                {
                    variables[i].Value = _possibleValues[i];
                }
            }
        }
    }
}
