using System;
using System.Collections.Generic;
using System.Text;
using qon.Machines;

namespace qon.Functions.Mutations
{
    public class CascadeMutation<TQ> : IMutationFunction<TQ> where TQ : notnull
    {
        private readonly List<IMutationFunction<TQ>> _mutations;

        public CascadeMutation(List<IMutationFunction<TQ>> mutations)
        {
            _mutations = mutations;
        }

        public List<Field<TQ>> ApplyTo(Field<TQ> input)
        {
            List<Field<TQ>> results = new List<Field<TQ>>{input};

            foreach (var mutation in _mutations)
            {
                List<Field<TQ>> temp = new List<Field<TQ>>();

                foreach (var field in results)
                {
                    temp.AddRange(mutation.ApplyTo(field));
                }

                if (temp.Count == 0)
                {
                    return temp;
                }
                
                results = temp;
            }

            return results;
        }
    }
}
