using qon.Functions;
using qon.Functions.Constraints;
using qon.Functions.Filters;
using qon.Functions.Propagators;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace qon.QSL
{
    public static class EuclideanPropagators
    {
        public static DefaultPropagator<VonNeumannParameter<TQ>> ToVonNeumann<TQ>(EuclideanConstraintParameter<TQ> param) where TQ : notnull
        {
            return new DefaultPropagator<VonNeumannParameter<TQ>>(vnp =>
            {
                int cumulativeChanges = 0;

                var leftResult = Propagators.ReduceDomainTo(param.CenterLevel[Side.Left]).ApplyTo(vnp.Left.FromNullableToArray());
                if (leftResult.Failed)
                {
                    return leftResult;
                }
                else
                {
                    cumulativeChanges += leftResult.ChangesAmount;
                }

                var rightResult = Propagators.ReduceDomainTo(param.CenterLevel[Side.Right]).ApplyTo(vnp.Right.FromNullableToArray());
                if (rightResult.Failed)
                {
                    return rightResult;
                }
                else
                {
                    cumulativeChanges += rightResult.ChangesAmount;
                }

                var frontResult = Propagators.ReduceDomainTo(param.CenterLevel[Side.Front]).ApplyTo(vnp.Front.FromNullableToArray());
                if (frontResult.Failed)
                {
                    return frontResult;
                }
                else
                {
                    cumulativeChanges += frontResult.ChangesAmount;
                }

                var backResult = Propagators.ReduceDomainTo(param.CenterLevel[Side.Back]).ApplyTo(vnp.Back.FromNullableToArray());
                if (backResult.Failed)
                {
                    return backResult;
                }
                else
                {
                    cumulativeChanges += backResult.ChangesAmount;
                }

                var topResult = Propagators.ReduceDomainTo(param[Slab.Top]).ApplyTo(vnp.Top.FromNullableToArray());
                if (topResult.Failed)
                {
                    return topResult;
                }
                else
                {
                    cumulativeChanges += topResult.ChangesAmount;
                }

                var bottomResult = Propagators.ReduceDomainTo(param[Slab.Bottom]).ApplyTo(vnp.Bottom.FromNullableToArray());
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
                if (!TryReduce(Propagators.ReduceDomainTo(param[Level.Top][Corner.FrontLeft]).ApplyTo(m.TopFrontLeft.FromNullableToArray()), ref cumulativeChanges))
                {
                    return Result.HasErrors();
                }

                if (!TryReduce(Propagators.ReduceDomainTo(param[Level.Top][Side.Front]).ApplyTo(m.TopFrontCenter.FromNullableToArray()), ref cumulativeChanges))
                {
                    return Result.HasErrors();
                }

                if (!TryReduce(Propagators.ReduceDomainTo(param[Level.Top][Corner.FrontRight]).ApplyTo(m.TopFrontRight.FromNullableToArray()), ref cumulativeChanges))
                {
                    return Result.HasErrors();
                }

                if (!TryReduce(Propagators.ReduceDomainTo(param[Level.Top][Side.Left]).ApplyTo(m.TopMedianLeft.FromNullableToArray()), ref cumulativeChanges))
                {
                    return Result.HasErrors();
                }

                if (!TryReduce(Propagators.ReduceDomainTo(param[Slab.Top]).ApplyTo(m.TopMedianCenter.FromNullableToArray()), ref cumulativeChanges))
                {
                    return Result.HasErrors();
                }

                if (!TryReduce(Propagators.ReduceDomainTo(param[Level.Top][Side.Right]).ApplyTo(m.TopMedianRight.FromNullableToArray()), ref cumulativeChanges))
                {
                    return Result.HasErrors();
                }

                if (!TryReduce(Propagators.ReduceDomainTo(param[Level.Top][Corner.BackRight]).ApplyTo(m.TopBackRight.FromNullableToArray()), ref cumulativeChanges))
                {
                    return Result.HasErrors();
                }

                if (!TryReduce(Propagators.ReduceDomainTo(param[Level.Top][Side.Back]).ApplyTo(m.TopBackCenter.FromNullableToArray()), ref cumulativeChanges))
                {
                    return Result.HasErrors();
                }

                if (!TryReduce(Propagators.ReduceDomainTo(param[Level.Top][Corner.BackLeft]).ApplyTo(m.TopBackLeft.FromNullableToArray()), ref cumulativeChanges))
                {
                    return Result.HasErrors();
                }

                //Middle layer
                if (!TryReduce(Propagators.ReduceDomainTo(param[Level.Middle][Corner.FrontLeft]).ApplyTo(m.MiddleFrontLeft.FromNullableToArray()), ref cumulativeChanges))
                {
                    return Result.HasErrors();
                }

                if (!TryReduce(Propagators.ReduceDomainTo(param[Level.Middle][Side.Front]).ApplyTo(m.MiddleFrontCenter.FromNullableToArray()), ref cumulativeChanges))
                {
                    return Result.HasErrors();
                }

                if (!TryReduce(Propagators.ReduceDomainTo(param[Level.Middle][Corner.FrontRight]).ApplyTo(m.MiddleFrontRight.FromNullableToArray()), ref cumulativeChanges))
                {
                    return Result.HasErrors();
                }

                if (!TryReduce(Propagators.ReduceDomainTo(param[Level.Middle][Side.Left]).ApplyTo(m.MiddleMedianLeft.FromNullableToArray()), ref cumulativeChanges))
                {
                    return Result.HasErrors();
                }

                if (!TryReduce(Propagators.ReduceDomainTo(param[Level.Middle][Side.Right]).ApplyTo(m.MiddleMedianRight.FromNullableToArray()), ref cumulativeChanges))
                {
                    return Result.HasErrors();
                }

                if (!TryReduce(Propagators.ReduceDomainTo(param[Level.Middle][Corner.BackRight]).ApplyTo(m.MiddleBackRight.FromNullableToArray()), ref cumulativeChanges))
                {
                    return Result.HasErrors();
                }

                if (!TryReduce(Propagators.ReduceDomainTo(param[Level.Middle][Side.Back]).ApplyTo(m.MiddleBackCenter.FromNullableToArray()), ref cumulativeChanges))
                {
                    return Result.HasErrors();
                }

                if (!TryReduce(Propagators.ReduceDomainTo(param[Level.Middle][Corner.BackLeft]).ApplyTo(m.MiddleBackLeft.FromNullableToArray()), ref cumulativeChanges))
                {
                    return Result.HasErrors();
                }

                //Bottom layer
                if (!TryReduce(Propagators.ReduceDomainTo(param[Level.Bottom][Corner.FrontLeft]).ApplyTo(m.BottomFrontLeft.FromNullableToArray()), ref cumulativeChanges))
                {
                    return Result.HasErrors();
                }

                if (!TryReduce(Propagators.ReduceDomainTo(param[Level.Bottom][Side.Front]).ApplyTo(m.BottomFrontCenter.FromNullableToArray()), ref cumulativeChanges))
                {
                    return Result.HasErrors();
                }

                if (!TryReduce(Propagators.ReduceDomainTo(param[Level.Bottom][Corner.FrontRight]).ApplyTo(m.BottomFrontRight.FromNullableToArray()), ref cumulativeChanges))
                {
                    return Result.HasErrors();
                }

                if (!TryReduce(Propagators.ReduceDomainTo(param[Level.Bottom][Side.Left]).ApplyTo(m.BottomMedianLeft.FromNullableToArray()), ref cumulativeChanges))
                {
                    return Result.HasErrors();
                }

                if (!TryReduce(Propagators.ReduceDomainTo(param[Slab.Bottom]).ApplyTo(m.BottomMedianCenter.FromNullableToArray()), ref cumulativeChanges))
                {
                    return Result.HasErrors();
                }

                if (!TryReduce(Propagators.ReduceDomainTo(param[Level.Bottom][Side.Right]).ApplyTo(m.BottomMedianRight.FromNullableToArray()), ref cumulativeChanges))
                {
                    return Result.HasErrors();
                }

                if (!TryReduce(Propagators.ReduceDomainTo(param[Level.Bottom][Corner.BackRight]).ApplyTo(m.BottomBackRight.FromNullableToArray()), ref cumulativeChanges))
                {
                    return Result.HasErrors();
                }

                if (!TryReduce(Propagators.ReduceDomainTo(param[Level.Bottom][Side.Back]).ApplyTo(m.BottomBackCenter.FromNullableToArray()), ref cumulativeChanges))
                {
                    return Result.HasErrors();
                }

                if (!TryReduce(Propagators.ReduceDomainTo(param[Level.Bottom][Corner.BackLeft]).ApplyTo(m.BottomBackLeft.FromNullableToArray()), ref cumulativeChanges))
                {
                    return Result.HasErrors();
                }

                return Result.Success(cumulativeChanges);
            });
        }
    }
}
