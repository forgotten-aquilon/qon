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
    public class ConstraintBuilder<TQ> : IPreparation<TQ> where TQ : notnull
    {
        public Func<QVariable<TQ>[], Result> Constraint { get; }

        public ConstraintBuilder(Func<QVariable<TQ>[], Result> constraint)
        {
            Constraint = constraint;
        }

        public Result Execute(Field<TQ> field)
        {
            return Constraint(field.Variables);
        }
    }
}
