using System;
using System.Collections.Generic;
using System.Linq;
using qon.Functions.Filters;
using qon.Functions.Propagators;
using qon.Helpers;
using qon.Variables;

namespace qon.Functions.Constraints
{
    public class EuclideanConstraint<T> : RelativeConstraint<T>
    {
        private EuclideanConstraint(
            QPredicate<T> guard,
            Propagator<T> propagator,
            Func<SuperpositionVariable<T>, QPredicate<T>> aggregationFactory)
            : base(guard, propagator, aggregationFactory)
        {
        }

        public EuclideanConstraint(QPredicate<T> guard, EuclideanConstraintParameter<T> parameter, Side side)
            : this(
                guard,
                Propagators.Propagators.DomainIntersectionWithHashSet(parameter[side]),
                CreateAggregationFactory(side))
        {
        }

        public EuclideanConstraint(QPredicate<T> guard, EuclideanConstraintParameter<T> parameter, Slab slab)
            : this(
                guard,
                Propagators.Propagators.DomainIntersectionWithHashSet(parameter[slab]),
                CreateAggregationFactory(slab))
        {
        }

        public static IEnumerable<EuclideanConstraint<T>> Create(QPredicate<T> guard, EuclideanConstraintParameter<T> parameter)
        {
            yield return new EuclideanConstraint<T>(guard, parameter, Side.Left);
            yield return new EuclideanConstraint<T>(guard, parameter, Side.Right);
            yield return new EuclideanConstraint<T>(guard, parameter, Side.Front);
            yield return new EuclideanConstraint<T>(guard, parameter, Side.Back);
            yield return new EuclideanConstraint<T>(guard, parameter, Slab.Top);
            yield return new EuclideanConstraint<T>(guard, parameter, Slab.Bottom);
        }

        private static Func<SuperpositionVariable<T>, QPredicate<T>> CreateAggregationFactory(Side side)
        {
            var offset = side switch
            {
                Side.Left => (-1, 0, 0),
                Side.Right => (1, 0, 0),
                Side.Front => (0, -1, 0),
                Side.Back => (0, 1, 0),
                _ => throw new ArgumentOutOfRangeException(nameof(side), side, null)
            };

            return CreateAggregationFactory(offset);
        }

        private static Func<SuperpositionVariable<T>, QPredicate<T>> CreateAggregationFactory(Slab slab)
        {
            var offset = slab switch
            {
                Slab.Top => (0, 0, 1),
                Slab.Bottom => (0, 0, -1),
                _ => throw new ArgumentOutOfRangeException(nameof(slab), slab, null)
            };

            return CreateAggregationFactory(offset);
        }

        private static Func<SuperpositionVariable<T>, QPredicate<T>> CreateAggregationFactory((int dx, int dy, int dz) offset)
        {
            return RelativeConstraint<T>.Create<EuclideanVariable<T>>(origin =>
                QPredicate<T>.Create<EuclideanVariable<T>>(candidate =>
                {
                    if (ReferenceEquals(candidate, origin))
                    {
                        return false;
                    }

                    return candidate.State == SuperpositionState.Uncertain
                        && candidate.X == origin.X + offset.dx
                        && candidate.Y == origin.Y + offset.dy
                        && candidate.Z == origin.Z + offset.dz;
                }));
        }
    }
}
