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
    //TODO: Rename
    public class ConstraintBuilder<TQ> : IPreparation<TQ> where TQ : notnull
    {
        public Func<QObject<TQ>[], Result> Constraint { get; }

        public ConstraintBuilder(Func<QObject<TQ>[], Result> constraint)
        {
            Constraint = constraint;
        }

        public Result Execute(Field<TQ> field)
        {
            return Constraint(field.Variables);
        }
    }
}
