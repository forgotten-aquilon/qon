using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Functions.Propagators;
using qon.Variables;

namespace qon.Functions.Constraints
{
    public class VonNeumannConstraint<T> : IQConstraint<T>
    {
        public ConstraintResult Execute(SuperpositionVariable<T>[] field)
        {
            throw new NotImplementedException();
        }
    }
}
