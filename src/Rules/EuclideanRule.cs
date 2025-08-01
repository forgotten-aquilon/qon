﻿using qon.Rules.Aggregators;
using qon.Rules.Guards;
using System;
using System.Collections.Generic;
using System.Linq;
using qon.Rules.Filters;

namespace qon.Rules
{
    public class EuclideanRule<T> : LocalRuleBase<T>
    {
        private readonly Func<SuperpositionVariable<T>, SelectingAggregator<T>> _leftAggregation = EuclideanAggregators.SelectRegion<T>((1, 0, 0), (0, 0, 0));
        private readonly Func<SuperpositionVariable<T>, SelectingAggregator<T>> _rightAggregation = EuclideanAggregators.SelectRegion<T>((0, 0, 0), (1, 0, 0));
        private readonly Func<SuperpositionVariable<T>, SelectingAggregator<T>> _frontAggregation = EuclideanAggregators.SelectRegion<T>((0, 1, 0), (0, 0, 0));
        private readonly Func<SuperpositionVariable<T>, SelectingAggregator<T>> _backAggregation = EuclideanAggregators.SelectRegion<T>((0, 0, 0), (0, 1, 0));
        private readonly Func<SuperpositionVariable<T>, SelectingAggregator<T>> _topAggregation = EuclideanAggregators.SelectRegion<T>((0, 0, 1), (0, 0, 0));
        private readonly Func<SuperpositionVariable<T>, SelectingAggregator<T>> _bottomAggregation = EuclideanAggregators.SelectRegion<T>((0, 0, 0), (0, 0, 1));

        private readonly EuclideanRuleParameter<T> _parameter;

        public EuclideanRule(List<Guard<T>> guards, EuclideanRuleParameter<T> parameter) : base(guards)
        {
            _parameter = parameter;
        }

        public override ConstraintResult Execute(List<SuperpositionVariable<T>> field, SuperpositionVariable<T> variable)
        {
            int changes = 0;
            var aggregation = field.Where(x => x.State == SuperpositionState.Uncertain);

            var left = aggregation.Where(_leftAggregation(variable).ApplyTo);
            var right = aggregation.Where(_rightAggregation(variable).ApplyTo);
            var front = aggregation.Where(_frontAggregation(variable).ApplyTo);
            var back = aggregation.Where(_backAggregation(variable).ApplyTo);
            var top = aggregation.Where(_topAggregation(variable).ApplyTo);
            var bottom = aggregation.Where(_bottomAggregation(variable).ApplyTo);
            
            List<ConstraintResult> results = new()
            {
                Filters.Filters.DomainIntersection(_parameter.Left).ApplyTo(left.ToList()),
                Filters.Filters.DomainIntersection(_parameter.Right).ApplyTo(right.ToList()),
                Filters.Filters.DomainIntersection(_parameter.Front).ApplyTo(front.ToList()),
                Filters.Filters.DomainIntersection(_parameter.Back).ApplyTo(back.ToList()),
                Filters.Filters.DomainIntersection(_parameter.Top).ApplyTo(top.ToList()),
                Filters.Filters.DomainIntersection(_parameter.Bottom).ApplyTo(bottom.ToList())
            };

            foreach (var result in results)
            {
                changes += result.ChangesAmount;

                if (result.Outcome == PropagationOutcome.Conflict)
                {
                    return result;
                }
            }

            return new ConstraintResult(PropagationOutcome.UnderConstrained, changes);
        }
    }
}
