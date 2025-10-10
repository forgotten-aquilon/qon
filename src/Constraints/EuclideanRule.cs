using System;
using System.Collections.Generic;
using System.Linq;
using qon.Constraints.Aggregators;
using qon.Constraints.Filters;
using qon.Helpers;
using qon.Variables;

namespace qon.Constraints
{
    public class EuclideanRule<T> : RelativeRule<T>
    {
        private EuclideanRule(
            SelectingAggregator<T> guard,
            Filter<T> filter,
            Func<SuperpositionVariable<T>, SelectingAggregator<T>> aggregationFactory)
            : base(guard, filter, aggregationFactory)
        {
        }

        public EuclideanRule(SelectingAggregator<T> guard, EuclideanRuleParameter<T> parameter, Side side)
            : this(
                guard,
                Filters.Filters.DomainIntersectionWithHashSet(parameter[side]),
                CreateAggregationFactory(side))
        {
        }

        public EuclideanRule(SelectingAggregator<T> guard, EuclideanRuleParameter<T> parameter, Slab slab)
            : this(
                guard,
                Filters.Filters.DomainIntersectionWithHashSet(parameter[slab]),
                CreateAggregationFactory(slab))
        {
        }

        public static IEnumerable<EuclideanRule<T>> Create(SelectingAggregator<T> guard, EuclideanRuleParameter<T> parameter)
        {
            yield return new EuclideanRule<T>(guard, parameter, Side.Left);
            yield return new EuclideanRule<T>(guard, parameter, Side.Right);
            yield return new EuclideanRule<T>(guard, parameter, Side.Front);
            yield return new EuclideanRule<T>(guard, parameter, Side.Back);
            yield return new EuclideanRule<T>(guard, parameter, Slab.Top);
            yield return new EuclideanRule<T>(guard, parameter, Slab.Bottom);
        }

        private static Func<SuperpositionVariable<T>, SelectingAggregator<T>> CreateAggregationFactory(Side side)
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

        private static Func<SuperpositionVariable<T>, SelectingAggregator<T>> CreateAggregationFactory(Slab slab)
        {
            var offset = slab switch
            {
                Slab.Top => (0, 0, 1),
                Slab.Bottom => (0, 0, -1),
                _ => throw new ArgumentOutOfRangeException(nameof(slab), slab, null)
            };

            return CreateAggregationFactory(offset);
        }

        private static Func<SuperpositionVariable<T>, SelectingAggregator<T>> CreateAggregationFactory((int dx, int dy, int dz) offset)
        {
            return RelativeRule<T>.Create<EuclideanVariable<T>>(origin =>
                SelectingAggregator<T>.Create<EuclideanVariable<T>>(candidate =>
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
