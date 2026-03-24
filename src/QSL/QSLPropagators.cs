using qon.Functions;
using qon.Functions.Constraints;
using qon.Functions.Filters;
using qon.Functions.Propagators;
using qon.Layers.StateLayers;
using qon.Layers.VariableLayers;
using qon.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using qon.Variables.Domains;

namespace qon
{
    public static partial class QSL
    {
        public static class Propagators
        {
            public static Result AllDistinctPropagator<TQ>(IEnumerable<QVariable<TQ>> variables) where TQ : notnull
            {
                int changes = 0;
                var allVariables = variables.ToArray();
                var decided = allVariables.Where(x => x.State != ValueState.Uncertain).Select(y => y.Value.Value).ToList();

                var certainVariablesCount = decided.Count;
                var distinctVariables = decided.ToHashSet();

                if (certainVariablesCount != distinctVariables.Count)
                {
                    return Result.HasErrors();
                }

                foreach (var variable in allVariables) if (variable.State == ValueState.Uncertain)
                {
                    changes += DomainLayer<TQ>.With(variable).RemoveValues(distinctVariables);
                    changes += ConstraintLayer<TQ>.TryCollapseVariable(variable).HasValue ? 1 : 0;
                }

                return Result.Success(changes);
            }

            public static Propagator<TQ> AllDistinct<TQ>() where TQ : notnull
            {
                return new Propagator<TQ>(AllDistinctPropagator);
            }

            public static Propagator<TQ> ReduceDomainTo<TQ>(HashSet<TQ> filteringCollection) where TQ : notnull
            {
                return new Propagator<TQ>(variables =>
                {
                    int changes = 0;

                    foreach (var variable in variables)
                    {
                        if (variable.State != ValueState.Uncertain)
                        {
                            continue;
                        }

                        int removed = DomainHelper<TQ>.DomainIntersectionWithHashSet(variable, filteringCollection);

                        if (DomainLayer<TQ>.With(variable).IsEmpty())
                        {
                            return Result.HasErrors();
                        }

                        changes += removed;
                        changes += ConstraintLayer<TQ>.TryCollapseVariable(variable).HasValue ? 1 : 0;
                    }

                    return Result.Success(changes);
                });
            }

            public static DefaultPropagator<bool> FromBool(bool invert = false)
            {
                return new DefaultPropagator<bool>(value => new Result(value ^ invert, 0));
            }

            public static DefaultPropagator<VonNeumannParameter<TQ>> ToVonNeumann<TQ>(EuclideanConstraintParameter<TQ> param) where TQ : notnull
            {
                return new DefaultPropagator<VonNeumannParameter<TQ>>(vnp =>
                {
                    int cumulativeChanges = 0;

                    var leftResult = ReduceDomainTo(param.CenterLevel[Side.Left]).ApplyTo(vnp.Left.FromNullableToArray());
                    if (leftResult.Failed)
                    {
                        return leftResult;
                    }
                    else
                    {
                        cumulativeChanges += leftResult.ChangesAmount;
                    }

                    var rightResult = ReduceDomainTo(param.CenterLevel[Side.Right]).ApplyTo(vnp.Right.FromNullableToArray());
                    if (rightResult.Failed)
                    {
                        return rightResult;
                    }
                    else
                    {
                        cumulativeChanges += rightResult.ChangesAmount;
                    }

                    var frontResult = ReduceDomainTo(param.CenterLevel[Side.Front]).ApplyTo(vnp.Front.FromNullableToArray());
                    if (frontResult.Failed)
                    {
                        return frontResult;
                    }
                    else
                    {
                        cumulativeChanges += frontResult.ChangesAmount;
                    }

                    var backResult = ReduceDomainTo(param.CenterLevel[Side.Back]).ApplyTo(vnp.Back.FromNullableToArray());
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

            public static DefaultPropagator<MooreParameter<TQ>> ToMoore<TQ>(EuclideanConstraintParameter<TQ> param) where TQ : notnull
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                bool TryReduce(in Result result, ref int changes)
                {
                    if (!result.Failed)
                    {
                        changes += result.ChangesAmount;
                        return true;
                    }

                    return false;
                }

                return new DefaultPropagator<MooreParameter<TQ>>(m =>
                {
                    int cumulativeChanges = 0;

                    //Top layer
                    if (!TryReduce(ReduceDomainTo(param[Level.Top][Corner.FrontLeft]).ApplyTo(m.TopFrontLeft.FromNullableToArray()), ref cumulativeChanges))
                    {
                        return Result.HasErrors();
                    }

                    if (!TryReduce(ReduceDomainTo(param[Level.Top][Side.Front]).ApplyTo(m.TopFrontCenter.FromNullableToArray()), ref cumulativeChanges))
                    {
                        return Result.HasErrors();
                    }

                    if (!TryReduce(ReduceDomainTo(param[Level.Top][Corner.FrontRight]).ApplyTo(m.TopFrontRight.FromNullableToArray()), ref cumulativeChanges))
                    {
                        return Result.HasErrors();
                    }

                    if (!TryReduce(ReduceDomainTo(param[Level.Top][Side.Left]).ApplyTo(m.TopMedianLeft.FromNullableToArray()), ref cumulativeChanges))
                    {
                        return Result.HasErrors();
                    }

                    if (!TryReduce(ReduceDomainTo(param[Slab.Top]).ApplyTo(m.TopMedianCenter.FromNullableToArray()), ref cumulativeChanges))
                    {
                        return Result.HasErrors();
                    }

                    if (!TryReduce(ReduceDomainTo(param[Level.Top][Side.Right]).ApplyTo(m.TopMedianRight.FromNullableToArray()), ref cumulativeChanges))
                    {
                        return Result.HasErrors();
                    }

                    if (!TryReduce(ReduceDomainTo(param[Level.Top][Corner.BackRight]).ApplyTo(m.TopBackRight.FromNullableToArray()), ref cumulativeChanges))
                    {
                        return Result.HasErrors();
                    }

                    if (!TryReduce(ReduceDomainTo(param[Level.Top][Side.Back]).ApplyTo(m.TopBackCenter.FromNullableToArray()), ref cumulativeChanges))
                    {
                        return Result.HasErrors();
                    }

                    if (!TryReduce(ReduceDomainTo(param[Level.Top][Corner.BackLeft]).ApplyTo(m.TopBackLeft.FromNullableToArray()), ref cumulativeChanges))
                    {
                        return Result.HasErrors();
                    }

                    //Middle layer
                    if (!TryReduce(ReduceDomainTo(param[Level.Middle][Corner.FrontLeft]).ApplyTo(m.MiddleFrontLeft.FromNullableToArray()), ref cumulativeChanges))
                    {
                        return Result.HasErrors();
                    }

                    if (!TryReduce(ReduceDomainTo(param[Level.Middle][Side.Front]).ApplyTo(m.MiddleFrontCenter.FromNullableToArray()), ref cumulativeChanges))
                    {
                        return Result.HasErrors();
                    }

                    if (!TryReduce(ReduceDomainTo(param[Level.Middle][Corner.FrontRight]).ApplyTo(m.MiddleFrontRight.FromNullableToArray()), ref cumulativeChanges))
                    {
                        return Result.HasErrors();
                    }

                    if (!TryReduce(ReduceDomainTo(param[Level.Middle][Side.Left]).ApplyTo(m.MiddleMedianLeft.FromNullableToArray()), ref cumulativeChanges))
                    {
                        return Result.HasErrors();
                    }

                    if (!TryReduce(ReduceDomainTo(param[Level.Middle][Side.Right]).ApplyTo(m.MiddleMedianRight.FromNullableToArray()), ref cumulativeChanges))
                    {
                        return Result.HasErrors();
                    }

                    if (!TryReduce(ReduceDomainTo(param[Level.Middle][Corner.BackRight]).ApplyTo(m.MiddleBackRight.FromNullableToArray()), ref cumulativeChanges))
                    {
                        return Result.HasErrors();
                    }

                    if (!TryReduce(ReduceDomainTo(param[Level.Middle][Side.Back]).ApplyTo(m.MiddleBackCenter.FromNullableToArray()), ref cumulativeChanges))
                    {
                        return Result.HasErrors();
                    }

                    if (!TryReduce(ReduceDomainTo(param[Level.Middle][Corner.BackLeft]).ApplyTo(m.MiddleBackLeft.FromNullableToArray()), ref cumulativeChanges))
                    {
                        return Result.HasErrors();
                    }

                    //Bottom layer
                    if (!TryReduce(ReduceDomainTo(param[Level.Bottom][Corner.FrontLeft]).ApplyTo(m.BottomFrontLeft.FromNullableToArray()), ref cumulativeChanges))
                    {
                        return Result.HasErrors();
                    }

                    if (!TryReduce(ReduceDomainTo(param[Level.Bottom][Side.Front]).ApplyTo(m.BottomFrontCenter.FromNullableToArray()), ref cumulativeChanges))
                    {
                        return Result.HasErrors();
                    }

                    if (!TryReduce(ReduceDomainTo(param[Level.Bottom][Corner.FrontRight]).ApplyTo(m.BottomFrontRight.FromNullableToArray()), ref cumulativeChanges))
                    {
                        return Result.HasErrors();
                    }

                    if (!TryReduce(ReduceDomainTo(param[Level.Bottom][Side.Left]).ApplyTo(m.BottomMedianLeft.FromNullableToArray()), ref cumulativeChanges))
                    {
                        return Result.HasErrors();
                    }

                    if (!TryReduce(ReduceDomainTo(param[Slab.Bottom]).ApplyTo(m.BottomMedianCenter.FromNullableToArray()), ref cumulativeChanges))
                    {
                        return Result.HasErrors();
                    }

                    if (!TryReduce(ReduceDomainTo(param[Level.Bottom][Side.Right]).ApplyTo(m.BottomMedianRight.FromNullableToArray()), ref cumulativeChanges))
                    {
                        return Result.HasErrors();
                    }

                    if (!TryReduce(ReduceDomainTo(param[Level.Bottom][Corner.BackRight]).ApplyTo(m.BottomBackRight.FromNullableToArray()), ref cumulativeChanges))
                    {
                        return Result.HasErrors();
                    }

                    if (!TryReduce(ReduceDomainTo(param[Level.Bottom][Side.Back]).ApplyTo(m.BottomBackCenter.FromNullableToArray()), ref cumulativeChanges))
                    {
                        return Result.HasErrors();
                    }

                    if (!TryReduce(ReduceDomainTo(param[Level.Bottom][Corner.BackLeft]).ApplyTo(m.BottomBackLeft.FromNullableToArray()), ref cumulativeChanges))
                    {
                        return Result.HasErrors();
                    }

                    return Result.Success(cumulativeChanges);
                });
            }
        }
    }
}
