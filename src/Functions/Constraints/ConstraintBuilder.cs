using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Functions.Propagators;
using qon.Machines;
using qon.Variables;

namespace qon.Functions.Constraints
{
    public class ConstraintBuilder<T> : IPreparation<T>
    {
        public Func<QVariable<T>[], Result> Constraint { get; }

        public ConstraintBuilder(Func<QVariable<T>[], Result> constraint)
        {
            Constraint = constraint;
        }

        public Result Execute(Field<T> field, QMachine<T>? machine)
        {
            return Constraint(field.Variables);
        }
    }
}
