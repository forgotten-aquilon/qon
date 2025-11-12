using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Variables;

namespace qon.Functions.Mutations
{
    public interface IMutator<T>
    {
        public int MutationCount { get; }
        public void Mutate(List<QVariable<T>> variables);
    }
}
