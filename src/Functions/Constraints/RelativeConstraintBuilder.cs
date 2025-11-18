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
    public class RelativeConstraintBuilder<TQ> : IPreparation<TQ> where TQ : notnull
    {
        protected QPredicate<TQ> Guard { get; }
        public Func<QVariable<TQ>, Result> Constraint { get; }

        public RelativeConstraintBuilder(QPredicate<TQ> guard, Func<QVariable<TQ>, Result> constraint)
        {
            Guard = guard;
            Constraint = constraint;
        }

        public Result Execute(Field<TQ> field)
        {
            List<QVariable<TQ>>? relativeVariables = field.Where(Guard.ApplyTo).ToList();

            int cumulativeChanges = 0;

            foreach (QVariable<TQ> relativeVariable in relativeVariables)
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
