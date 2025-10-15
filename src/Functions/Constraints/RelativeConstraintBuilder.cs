using qon.Functions.Filters;
using qon.Functions.Propagators;
using qon.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qon.Functions.Constraints
{
    public class RelativeConstraintBuilder<T> : IQConstraint<T>
    {
        protected QPredicate<T> Guard { get; }
        public Func<SuperpositionVariable<T>, ConstraintResult> Constraint { get; }

        public RelativeConstraintBuilder(QPredicate<T> guard, Func<SuperpositionVariable<T>, ConstraintResult> constraint)
        {
            Guard = guard;
            Constraint = constraint;
        }

        public ConstraintResult Execute(SuperpositionVariable<T>[] field)
        {
            List<SuperpositionVariable<T>>? relativeVariables = field.Where(Guard.ApplyTo).ToList();

            int cumulativeChanges = 0;

            foreach (SuperpositionVariable<T> relativeVariable in relativeVariables)
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

            return ConstraintResult.Success(cumulativeChanges);
        }
    }
}
