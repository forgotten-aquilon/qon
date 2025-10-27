using qon.Domains;
using qon.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qon.Machines
{
    public class RuleHandler<T>
    {
        public List<IPreparation<T>> GeneralConstraints { get; set; } = new List<IPreparation<T>>();
        public List<IPreparation<T>>? ValidationConstraints { get; set; } = new List<IPreparation<T>>();
    }

    public class WFCParameter<T> : QMachineParameter<T>
    {
        public IDomain<T> Domain { get; set; } = EmptyDomain<T>.Instance;
        public RuleHandler<T> Constraints { get; set; } = new();
    }
}
