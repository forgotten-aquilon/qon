using System.Collections.Generic;
using System.Linq;
using qon.Domains;
using qon.Exceptions;
using qon.Helpers;
using qon.Variables;

namespace qon.Rules.Filters
{
    public static class Filters
    {
        public static ConstraintResult AllDistinctFilter<T>(IEnumerable<SuperpositionVariable<T>> filteringList)
        {
            int changes = 0;

            var decided = filteringList.Where(x => x.State != SuperpositionState.Uncertain).Select(y => y.Value.Value);

            var certainVariablesCount = decided.Count();
            var distinctVariables = decided.ToHashSet();

            if (certainVariablesCount != distinctVariables.Count)
            {
                return new ConstraintResult(PropagationOutcome.Conflict, 0);
            }

            var openVariables = filteringList.Where(x => x.State == SuperpositionState.Uncertain);

            int convergedCount = 0;
            int openVariablesCount = 0;
            foreach (var variable in openVariables)
            {
                changes += variable.RemoveFromDomain(distinctVariables);
                if (variable.AutoCollapse().HasValue)
                {
                    convergedCount++;
                }
                openVariablesCount++;
            }

            return (convergedCount == openVariablesCount) switch
            {
                true => new ConstraintResult(PropagationOutcome.Converged, changes),
                false => new ConstraintResult(PropagationOutcome.UnderConstrained, changes)
            };
        }

        public static Filter<T> AllDistinct<T>()
        {
            return new Filter<T>(AllDistinctFilter);
        }

        public static Filter<T> DomainIntersection<T>(IEnumerable<T> filteringCollection)
        {
            //Add equal discrete domain
            return DomainIntersection(new DiscreteDomain<T>(filteringCollection));
        }

        public static Filter<T> DomainIntersection<T>(DiscreteDomain<T> filteringDomain)
        {
            return new Filter<T>(list =>
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
                        return new ConstraintResult(PropagationOutcome.Conflict, 0);
                    }

                    if (variable.AutoCollapse() != Optional<T>.Empty || originalSize-newDomain.Size() != 0)
                    {
                        changes++;
                    }
                }

                return list.All(x => x.State != SuperpositionState.Uncertain) switch
                {
                    true => new ConstraintResult(PropagationOutcome.Converged, changes),
                    false => new ConstraintResult(PropagationOutcome.UnderConstrained, changes)
                };
            });
        }

        public static Filter<T> AmountCheck<T>(int value, Comparison condition)
        {
            return new Filter<T>(list =>
            {
                int count = list.Count();
                bool result = condition switch
                {
                    Comparison.EQ => count == value,
                    Comparison.NE => count != value,
                    Comparison.LT => count < value,
                    Comparison.LE => count <= value,
                    Comparison.GT => count > value,
                    Comparison.GE => count >= value,
                    _ => throw new InternalLogicException("Passed nonexistent enum value"),
                };

                return result switch
                {
                    true => new ConstraintResult(PropagationOutcome.Converged, 0),
                    false => new ConstraintResult(PropagationOutcome.Conflict, 0)
                };
            });
        }
    }
}
