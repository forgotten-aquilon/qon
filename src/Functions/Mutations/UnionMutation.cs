using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using qon.Machines;

namespace qon.Functions.Mutations
{
    public class UnionMutation<TQ> : MutationFunction<TQ> where TQ : notnull
    {
        private readonly List<MutationFunction<TQ>> _mutations;

        public UnionMutation(params MutationFunction<TQ>[] mutations)
        {
            _mutations = mutations.ToList();
        }

        public override List<Field<TQ>> ApplyTo(Field<TQ> input)
        {
            List<Field<TQ>> result = new List<Field<TQ>>();

            foreach (var mutation in _mutations)
            {
                result.AddRange(mutation.ApplyTo(input));
            }

            return result;
        }
    }
}
