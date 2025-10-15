using System.Collections.Generic;
using System.Linq;
using qon.Domains;
using qon.Exceptions;
using qon.Functions.Constraints;
using qon.Functions.Filters;
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
                return ConstraintResult.HasErrors();
            }

            var openVariables = field.Where(x => x.State == SuperpositionState.Uncertain);

            foreach (var variable in openVariables)
            {
                changes += variable.RemoveFromDomain(distinctVariables);
                changes += variable.AutoCollapse().HasValue ? 1 : 0;
            }

            return ConstraintResult.Success(changes);
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
                        return ConstraintResult.HasErrors();
                    }

                    if (variable.AutoCollapse().HasValue || removed > 0)
                    {
                        changes += removed;
                    }
                }

                return ConstraintResult.Success(changes);
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
                        return ConstraintResult.HasErrors();
                    }

                    if (variable.AutoCollapse() != Optional<T>.Empty || originalSize-newDomain.Size() != 0)
                    {
                        changes++;
                    }
                }

                return ConstraintResult.Success(changes);
            });
        }

        public static DefaultPropagator<bool> FromBool(bool invert = false)
        {
            return new DefaultPropagator<bool>(value => new ConstraintResult(value ^ invert, 0));
        }

        public static DefaultPropagator<VonNeumannParameter<T>> FromVonNeumann<T>(EuclideanConstraintParameter<T> param)
        {
            return new DefaultPropagator<VonNeumannParameter<T>>(vnp =>
            {
                //TODO optimize
                int cumulativeChanges = 0;

                var leftResult = DomainIntersectionWithHashSet(param[Side.Left]).ApplyTo(vnp.Left.FromNullableToArray());
                if (leftResult.Failed)
                {
                    return leftResult;
                }
                else
                {
                    cumulativeChanges += leftResult.ChangesAmount;
                }

                var rightResult = DomainIntersectionWithHashSet(param[Side.Right]).ApplyTo(vnp.Right.FromNullableToArray());
                if (rightResult.Failed)
                {
                    return rightResult;
                }
                else
                {
                    cumulativeChanges += rightResult.ChangesAmount;
                }

                var frontResult = DomainIntersectionWithHashSet(param[Side.Front]).ApplyTo(vnp.Front.FromNullableToArray());
                if (frontResult.Failed)
                {
                    return frontResult;
                }
                else
                {
                    cumulativeChanges += frontResult.ChangesAmount;
                }

                var backResult = DomainIntersectionWithHashSet(param[Side.Back]).ApplyTo(vnp.Back.FromNullableToArray());
                if (backResult.Failed)
                {
                    return backResult;
                }
                else
                {
                    cumulativeChanges += backResult.ChangesAmount;
                }

                var topResult = DomainIntersectionWithHashSet(param[Slab.Top]).ApplyTo(vnp.Top.FromNullableToArray());
                if (topResult.Failed)
                {
                    return topResult;
                }
                else
                {
                    cumulativeChanges += topResult.ChangesAmount;
                }

                var bottomResult = DomainIntersectionWithHashSet(param[Slab.Bottom]).ApplyTo(vnp.Bottom.FromNullableToArray());
                if (bottomResult.Failed)
                {
                    return bottomResult;
                }
                else
                {
                    cumulativeChanges += bottomResult.ChangesAmount;
                }

                return ConstraintResult.Success(cumulativeChanges);
            });
        }       
    }
}
