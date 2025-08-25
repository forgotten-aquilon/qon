using System;
using System.Collections.Generic;
using qon.Domains;
using qon.Rules;
using qon.Variables;

namespace qon
{
    public class FieldParameter<T>
    {
        public IDomain<T>? Domain { get; set; }

        public IEnumerable<SuperpositionVariable<T>>? Field { get; set; }
    }

    public class RuleHandler<T>
    {
        public List<IGlobalRule<T>> GlobalRules { get; set; } = new List<IGlobalRule<T>>();
        public List<ILocalRule<T>> LocalRules { get; set; } = new List<ILocalRule<T>>();
    }

    public class QMachineParameter<T>
    {
        public FieldParameter<T>? FieldParameter { get; set; } = null;
        public RuleHandler<T> GeneralRules { get; set; } = new();
        public RuleHandler<T>? ValidationRules { get; set; }
        public Random Random { get; set; } = new Random();
    }
}
