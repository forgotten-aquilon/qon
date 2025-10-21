using System.Collections.Generic;
using System.Linq;
using qon.Domains;
using qon.Exceptions;
using qon.Functions.Constraints;
using qon.Functions.Filters;
using qon.Functions.Operations;
using qon.Helpers;
using qon.Layers.StateLayers;
using qon.Layers.VariableLayers;
using qon.Variables;

namespace qon.Functions.Propagators
{
    public static class Propagators
    {
        public static Result AllDistinctPropagator<T>(IEnumerable<QVariable<T>> field)
        {
            int changes = 0;

            var decided = field.Where(x => x.State != ValueState.Uncertain).Select(y => y.Value.Value);

            var certainVariablesCount = decided.Count();
            var distinctVariables = decided.ToHashSet();

            if (certainVariablesCount != distinctVariables.Count)
            {
                return Result.HasErrors();
            }

            var openVariables = field.Where(x => x.State == ValueState.Uncertain);

            foreach (var variable in openVariables)
            {
                changes += DomainLayer<T>.With(variable).RemoveValues(distinctVariables);
                changes += ConstraintLayer<T>.TryCollapseVariable(variable).HasValue ? 1 : 0;
            }

            return Result.Success(changes);
        }

        public static Propagator<T> AllDistinct<T>()
        {
            return new Propagator<T>(AllDistinctPropagator);
        }

        public static Propagator<T> DomainIntersection<T>(HashSet<T> filteringCollection)
        {
            ExceptionHelper.ThrowIfArgumentIsNull(filteringCollection, nameof(filteringCollection));

            return new Propagator<T>(field =>
            {
                int changes = 0;

                foreach (var variable in field)
                {
                    if (variable.State != ValueState.Uncertain)
                    {
                        continue;
                    }

                    int removed = DomainHelper<T>.DomainIntersectionWithHashSet(variable, filteringCollection);

                    if (DomainLayer<T>.With(variable).IsEmpty())
                    {
                        return Result.HasErrors();
                    }

                    if (ConstraintLayer<T>.TryCollapseVariable(variable).HasValue || removed > 0)
                    {
                        changes += removed;
                    }
                }

                return Result.Success(changes);
            });
        }

        public static DefaultPropagator<bool> FromBool(bool invert = false)
        {
            return new DefaultPropagator<bool>(value => new Result(value ^ invert, 0));
        }

        public static DefaultPropagator<VonNeumannParameter<T>> FromVonNeumann<T>(EuclideanConstraintParameter<T> param)
        {
            return new DefaultPropagator<VonNeumannParameter<T>>(vnp =>
            {
                //TODO optimize
                int cumulativeChanges = 0;

                var leftResult = DomainIntersection(param[Side.Left]).ApplyTo(vnp.Left.FromNullableToArray());
                if (leftResult.Failed)
                {
                    return leftResult;
                }
                else
                {
                    cumulativeChanges += leftResult.ChangesAmount;
                }

                var rightResult = DomainIntersection(param[Side.Right]).ApplyTo(vnp.Right.FromNullableToArray());
                if (rightResult.Failed)
                {
                    return rightResult;
                }
                else
                {
                    cumulativeChanges += rightResult.ChangesAmount;
                }

                var frontResult = DomainIntersection(param[Side.Front]).ApplyTo(vnp.Front.FromNullableToArray());
                if (frontResult.Failed)
                {
                    return frontResult;
                }
                else
                {
                    cumulativeChanges += frontResult.ChangesAmount;
                }

                var backResult = DomainIntersection(param[Side.Back]).ApplyTo(vnp.Back.FromNullableToArray());
                if (backResult.Failed)
                {
                    return backResult;
                }
                else
                {
                    cumulativeChanges += backResult.ChangesAmount;
                }

                var topResult = DomainIntersection(param[Slab.Top]).ApplyTo(vnp.Top.FromNullableToArray());
                if (topResult.Failed)
                {
                    return topResult;
                }
                else
                {
                    cumulativeChanges += topResult.ChangesAmount;
                }

                var bottomResult = DomainIntersection(param[Slab.Bottom]).ApplyTo(vnp.Bottom.FromNullableToArray());
                if (bottomResult.Failed)
                {
                    return bottomResult;
                }
                else
                {
                    cumulativeChanges += bottomResult.ChangesAmount;
                }

                return Result.Success(cumulativeChanges);
            });
        }       
    }
}
