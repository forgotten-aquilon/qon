using qon.Machines;
using qon.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Solvers;

namespace qon.Functions
{
    public interface IPreValidation<TQ> where TQ : notnull
    {
        public PreValidationResult Execute(QVariable<TQ>[] field, QMachine<TQ>? machine = null);
    }
}
