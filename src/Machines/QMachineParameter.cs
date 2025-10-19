using System;
using System.Collections.Generic;
using qon.Domains;
using qon.Functions.Constraints;
using qon.Variables;

namespace qon.Machines
{
    public class QMachineParameter<T>
    {
        public IEnumerable<QVariable<T>>? Field { get; set; }
        public Random Random { get; set; } = new Random();
    }
}
