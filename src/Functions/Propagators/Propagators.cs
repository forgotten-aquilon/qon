using System.Collections.Generic;
using System.Linq;
using qon.Exceptions;
using qon.Functions.Constraints;
using qon.Functions.Filters;
using qon.Functions.Operations;
using qon.Helpers;
using qon.Layers.StateLayers;
using qon.Layers.VariableLayers;
using qon.Variables;
using qon.Variables.Domains;

namespace qon.Functions.Propagators
{
    public static class Propagators
    {
        public static Result AllDistinctPropagator<T>(IEnumerable<QVariable<T>> variables)
        {
            int changes = 0;

            var decided = variables.Where(x => x.State != ValueState.Uncertain).Select(y => y.Value.Value);

            var certainVariablesCount = decided.Count();
            var distinctVariables = decided.ToHashSet();

            if (certainVariablesCount != distinctVariables.Count)
            {
                return Result.HasErrors();
            }

            var openVariables = variables.Where(x => x.State == ValueState.Uncertain);

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

        public static Propagator<T> ReduceDomainTo<T>(HashSet<T> filteringCollection)
        {
            return new Propagator<T>(variables =>
            {
                int changes = 0;

                foreach (var variable in variables)
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

                var leftResult = ReduceDomainTo(param[Side.Left]).ApplyTo(vnp.Left.FromNullableToArray());
                if (leftResult.Failed)
                {
                    return leftResult;
                }
                else
                {
                    cumulativeChanges += leftResult.ChangesAmount;
                }

                var rightResult = ReduceDomainTo(param[Side.Right]).ApplyTo(vnp.Right.FromNullableToArray());
                if (rightResult.Failed)
                {
                    return rightResult;
                }
                else
                {
                    cumulativeChanges += rightResult.ChangesAmount;
                }

                var frontResult = ReduceDomainTo(param[Side.Front]).ApplyTo(vnp.Front.FromNullableToArray());
                if (frontResult.Failed)
                {
                    return frontResult;
                }
                else
                {
                    cumulativeChanges += frontResult.ChangesAmount;
                }

                var backResult = ReduceDomainTo(param[Side.Back]).ApplyTo(vnp.Back.FromNullableToArray());
                if (backResult.Failed)
                {
                    return backResult;
                }
                else
                {
                    cumulativeChanges += backResult.ChangesAmount;
                }

                var topResult = ReduceDomainTo(param[Slab.Top]).ApplyTo(vnp.Top.FromNullableToArray());
                if (topResult.Failed)
                {
                    return topResult;
                }
                else
                {
                    cumulativeChanges += topResult.ChangesAmount;
                }

                var bottomResult = ReduceDomainTo(param[Slab.Bottom]).ApplyTo(vnp.Bottom.FromNullableToArray());
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
