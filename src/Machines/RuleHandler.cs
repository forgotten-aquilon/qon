using System.Collections.Generic;
using qon.Functions;

namespace qon.Machines
{
    public class RuleHandler<T>
    {
        public List<IPreparation<T>> GeneralConstraints { get; set; } = new List<IPreparation<T>>();
        public List<IPreparation<T>>? ValidationConstraints { get; set; } = new List<IPreparation<T>>();
    }
}