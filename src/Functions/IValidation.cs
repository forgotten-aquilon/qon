using qon.Machines;
using qon.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qon.Functions
{
    public interface IValidation<TQ> where TQ : notnull
    {
        public bool Execute(QVariable<TQ>[] field, QMachine<TQ>? machine = null);
    }
}
