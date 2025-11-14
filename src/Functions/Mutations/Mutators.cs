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
        public class DefaultMutator<T> : IMutator<T>
        {
            private readonly List<VariableMutation<T>> _mutations;
            public int MutationCount => _mutations.Count;

            public DefaultMutator(List<VariableMutation<T>> mutations)
            {
                _mutations = mutations;
            }

            public void Mutate(List<QVariable<T>> variables)
            {
                for (int i = 0; i < variables.Count; i++)
                {
                    _mutations[i].Execute(variables[i]);
                }
            }
        }


        public class ValueMutator<T> : IMutator<T>
        {
            private readonly List<T> _possibleValues;
            public int MutationCount => _possibleValues.Count;

            public ValueMutator(IEnumerable<T> possibleValues)
            {
                _possibleValues = possibleValues.ToList();
            }

            public ValueMutator(params T[] possibleValues)
            {
                _possibleValues = possibleValues.ToList();
            }

            public void Mutate(List<QVariable<T>> variables)
            {
                for (int i = 0; i < variables.Count; i++)
                {
                    variables[i].Value = Optional<T>.Of(_possibleValues[i]);
                }
            }
        }
    }
}
