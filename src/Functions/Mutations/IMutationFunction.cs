using qon.Machines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qon.Functions.Mutations
{
    public interface IMutationFunction<TQ> : IChain<Field<TQ>, List<Field<TQ>>> where TQ : notnull
    {
    }
}
