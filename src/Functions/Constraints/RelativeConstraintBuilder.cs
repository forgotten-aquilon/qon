using qon.Functions.Filters;
using qon.Functions.Propagators;
using qon.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Machines;

namespace qon.Functions.Constraints
{
    public class RelativeConstraintBuilder<T> : IPreparation<T>
    {
        protected QPredicate<T> Guard { get; }
        public Func<QVariable<T>, Result> Constraint { get; }

        public RelativeConstraintBuilder(QPredicate<T> guard, Func<QVariable<T>, Result> constraint)
        {
            Guard = guard;
            Constraint = constraint;
        }

        public Result Execute(Field<T> field, QMachine<T>? machine)
        {
            List<QVariable<T>>? relativeVariables = field.Where(Guard.ApplyTo).ToList();

            int cumulativeChanges = 0;

            foreach (QVariable<T> relativeVariable in relativeVariables)
            {
                var result = Constraint(relativeVariable);

                if (result.Failed)
                {
                    return result;
                }
                else
                {
                    cumulativeChanges += result.ChangesAmount;
                }
            }

            return Result.Success(cumulativeChanges);
        }
    }
}
