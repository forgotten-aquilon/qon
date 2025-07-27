using System;
using System.Collections.Generic;
using qon.Domains;
using qon.Rules;

namespace qon
{
    public class FieldParameter<T>
    {
        public IDomain<T> Domain { get; set; }

        public IEnumerable<SuperpositionVariable<T>> Field { get; set; }
    }

    public class QMachineParameter<T>
    {
        public FieldParameter<T>? FieldParameter { get; set; } = null;
        public List<IGlobalRule<T>> GlobalRules { get; set; } = new List<IGlobalRule<T>>();
        public List<ILocalRule<T>> VariableRules { get; set; } = new List<ILocalRule<T>>();
        public Random Random { get; set; } = new Random();
    }
}
