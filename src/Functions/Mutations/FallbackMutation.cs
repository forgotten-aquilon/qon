using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using qon.Machines;

namespace qon.Functions.Mutations
{
    public class FallbackMutation<TQ> : IMutationFunction<TQ> where TQ : notnull
    {
        private readonly List<IMutationFunction<TQ>> _mutations;

        public FallbackMutation(params IMutationFunction<TQ>[] mutations)
        {
            _mutations = mutations.ToList();
        }

        public List<Field<TQ>> ApplyTo(Field<TQ> input)
        {
            foreach (var mutation in _mutations)
            {
                if (mutation.ApplyTo(input) is {Count:>0} result)
                {
                    return result;
                }
            }

            return new List<Field<TQ>>();
        }
    }
}
