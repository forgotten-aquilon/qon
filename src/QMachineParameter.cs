using System;
using System.Collections.Generic;
using qon.Domains;
using qon.Functions.Constraints;
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
        public List<IQConstraint<T>> GeneralConstraints { get; set; } = new List<IQConstraint<T>>();
        public List<IQConstraint<T>>? ValidationConstraints { get; set; } = new List<IQConstraint<T>>();
    }

    public class QMachineParameter<T>
    {
        public FieldParameter<T>? FieldParameter { get; set; } = null;
        public RuleHandler<T> Constraints { get; set; } = new();
        public Random Random { get; set; } = new Random();
    }
}
