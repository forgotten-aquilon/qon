using qon.Machines;
using qon.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qon.Functions
{
    public interface IValidation<T>
    {
        public bool Execute(QVariable<T>[] field, QMachine<T>? machine = null);
    }
}
