using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Functions.Propagators;
using qon.Variables;

namespace qon.Functions.Constraints
{
    public class ConstraintBuilder<T> : IQConstraint<T>
    {
        public Func<QVariable<T>[], ConstraintResult> Constraint { get; }

        public ConstraintBuilder(Func<QVariable<T>[], ConstraintResult> constraint)
        {
            Constraint = constraint;
        }

        public ConstraintResult Execute(QVariable<T>[] field)
        {
            return Constraint(field);
        }
    }
}
