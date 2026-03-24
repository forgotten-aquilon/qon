using System;
using System.Collections.Generic;
using System.Text;
using qon.Machines;

namespace qon.Functions.Mutations
{
    public class CascadeMutation<TQ> : MutationFunction<TQ> where TQ : notnull
    {
        private readonly List<MutationFunction<TQ>> _mutations;

        public CascadeMutation(List<MutationFunction<TQ>> mutations)
        {
            _mutations = mutations;
        }

        public override List<Field<TQ>> ApplyTo(Field<TQ> input)
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
