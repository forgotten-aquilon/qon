using System.Collections.Generic;
using System.Linq;
using qon.Domains;
using qon.Exceptions;
using qon.Functions.Operations;
using qon.Helpers;
using qon.Variables;

namespace qon.Functions.Propagators
{
    public static class Propagators
    {
        public static ConstraintResult AllDistinctPropagator<T>(IEnumerable<SuperpositionVariable<T>> field)
        {
            int changes = 0;

            var decided = field.Where(x => x.State != SuperpositionState.Uncertain).Select(y => y.Value.Value);

            var certainVariablesCount = decided.Count();
            var distinctVariables = decided.ToHashSet();

            if (certainVariablesCount != distinctVariables.Count)
            {
                return new ConstraintResult(false, 0);
            }

            var openVariables = field.Where(x => x.State == SuperpositionState.Uncertain);

            foreach (var variable in openVariables)
            {
                changes += variable.RemoveFromDomain(distinctVariables);
                changes += variable.AutoCollapse().HasValue ? 1 : 0;
            }

            return new ConstraintResult(true, changes);
        }

        public static Propagator<T> AllDistinct<T>()
        {
            return new Propagator<T>(AllDistinctPropagator);
        }

        public static Propagator<T> DomainIntersectionWithHashSet<T>(HashSet<T> filteringCollection)
        {
            ExceptionHelper.ThrowIfArgumentIsNull(filteringCollection, nameof(filteringCollection));

            return new Propagator<T>(field =>
            {
                int changes = 0;

                foreach (var variable in field)
                {
                    if (variable.State != SuperpositionState.Uncertain)
                    {
                        continue;
                    }

                    int removed = DomainHelper<T>.DomainIntersectionWithHashSet(variable, filteringCollection);

                    if (variable.Domain.IsEmpty())
                    {
                        return new ConstraintResult(false, 0);
                    }

                    if (variable.AutoCollapse().HasValue || removed > 0)
                    {
                        changes += removed;
                    }
                }

                return new ConstraintResult(true, changes);
            });
        }

        public static Propagator<T> DomainIntersection<T>(DiscreteDomain<T> filteringDomain)
        {
            return new Propagator<T>(list =>
            {
                int changes = 0;

                foreach (var variable in list)
                {
                    if (variable.State != SuperpositionState.Uncertain)
                    {
                        continue;
                    }

                    int originalSize = variable.Domain.Size();

#pragma warning disable CS8714
                    IDomain<T> newDomain = DomainHelper<T>.DomainIntersection(variable.Domain, filteringDomain);
                    variable.Domain = newDomain;

                    if (newDomain.IsEmpty())
                    {
                        return new ConstraintResult(false, 0);
                    }

                    if (variable.AutoCollapse() != Optional<T>.Empty || originalSize-newDomain.Size() != 0)
                    {
                        changes++;
                    }
                }

                return new ConstraintResult(true, changes);
            });
        }

        public static DefaultPropagator<T, bool> AsConstraint<T>()
        {
            return new DefaultPropagator<T, bool>(value => new ConstraintResult(value, 0));
        }

        public static Propagator<T> AmountCheck<T>(int value, COperator condition)
        {
            return new Propagator<T>(list =>
            {
                int count = list.Count();
                bool result = condition switch
                {
                    COperator.EQ => count == value,
                    COperator.NE => count != value,
                    COperator.LT => count < value,
                    COperator.LE => count <= value,
                    COperator.GT => count > value,
                    COperator.GE => count >= value,
                    _ => throw new InternalLogicException("Passed nonexistent enum value"),
                };

                return new ConstraintResult(result, 0);
            });
        }
    }
}
