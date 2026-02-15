using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Variables;

namespace qon.Functions.Mutations
{
    //TODO: turn into single-value mutator
    public interface IMutator<TQ> where TQ : notnull
    {
        public int MutationCount { get; }
        public void Mutate(List<QVariable<TQ>> variables);
    }
}
