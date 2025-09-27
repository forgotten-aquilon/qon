using qon.Exceptions;
using qon.Rules.Aggregators;
using qon.Rules.Filters;
using qon.Rules.Guards;
using qon.Helpers;
using qon.Variables;
using System;
using System.Collections.Generic;
using System.Linq;

namespace qon.Rules
{
    public class EuclideanRule<T> : LocalRuleBase<T>
    {
        private readonly EuclideanRuleParameter<T> _parameter;

        public EuclideanRule(List<Guard<T>> guards, EuclideanRuleParameter<T> parameter) : base(guards)
        {
            _parameter = parameter;
        }

        public override ConstraintResult Execute(List<SuperpositionVariable<T>> field, SuperpositionVariable<T> variable)
        {
            //Optimization
            var euclideanVar = ExceptionHelper.CheckIfTypesMismatch<EuclideanVariable<T>>(variable);
            if (euclideanVar.HasValue)
            {
                return EuclideanExecute(euclideanVar.Value);
            }

            var aggregation = field.Where(x => x.State == SuperpositionState.Uncertain && x.Name != variable.Name &&
                Math.Abs((int)x["x"] - (int)variable["x"]) + Math.Abs((int)x["y"] - (int)variable["y"]) + Math.Abs((int)x["z"] - (int)variable["z"]) <= 3);

            int changes = 0;

            var left = aggregation.Where(x => (int)x["x"] == (int)variable["x"] - 1 &&
                                              (int)x["y"] == (int)variable["y"] &&
                                              (int)x["z"] == (int)variable["z"]);
            var right = aggregation.Where(x => (int)x["x"] == (int)variable["x"] + 1 &&
                                               (int)x["y"] == (int)variable["y"] &&
                                               (int)x["z"] == (int)variable["z"]);
            var front = aggregation.Where(x => (int)x["y"] == (int)variable["y"] - 1 &&
                                               (int)x["x"] == (int)variable["x"] &&
                                               (int)x["z"] == (int)variable["z"]);
            var back = aggregation.Where(x => (int)x["y"] == (int)variable["y"] + 1 &&
                                              (int)x["x"] == (int)variable["x"] &&
                                              (int)x["z"] == (int)variable["z"]);
            var top = aggregation.Where(x => (int)x["z"] == (int)variable["z"] - 1 &&
                                             (int)x["y"] == (int)variable["y"] &&
                                             (int)x["x"] == (int)variable["x"]);
            var bottom = aggregation.Where(x => (int)x["z"] == (int)variable["z"] + 1 &&
                                             (int)x["y"] == (int)variable["y"] &&
                                             (int)x["x"] == (int)variable["x"]);

            //var left = aggregation.Where(_leftAggregation(variable).ApplyTo);
            //var right = aggregation.Where(_rightAggregation(variable).ApplyTo);
            //var front = aggregation.Where(_frontAggregation(variable).ApplyTo);
            //var back = aggregation.Where(_backAggregation(variable).ApplyTo);
            //var top = aggregation.Where(_topAggregation(variable).ApplyTo);
            //var bottom = aggregation.Where(_bottomAggregation(variable).ApplyTo);
            
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

        protected virtual ConstraintResult EuclideanExecute(EuclideanVariable<T> variable)
        {
            var left = variable.Machine[(variable.X - 1, variable.Y, variable.Z)];
            var right = variable.Machine[(variable.X + 1, variable.Y, variable.Z)];
            var front = variable.Machine[(variable.X, variable.Y - 1, variable.Z)];
            var back = variable.Machine[(variable.X, variable.Y + 1, variable.Z)];
            var top = variable.Machine[(variable.X, variable.Y, variable.Z + 1)];
            var bottom = variable.Machine[(variable.X, variable.Y, variable.Z - 1)];

            List<ConstraintResult> results = new()
            {
                Filters.Filters.DomainIntersection(_parameter.Left).ApplyTo(left.FromNullableToArray()),
                Filters.Filters.DomainIntersection(_parameter.Right).ApplyTo(right.FromNullableToArray()),
                Filters.Filters.DomainIntersection(_parameter.Front).ApplyTo(front.FromNullableToArray()),
                Filters.Filters.DomainIntersection(_parameter.Back).ApplyTo(back.FromNullableToArray()),
                Filters.Filters.DomainIntersection(_parameter.Top).ApplyTo(top.FromNullableToArray()),
                Filters.Filters.DomainIntersection(_parameter.Bottom).ApplyTo(bottom.FromNullableToArray())
            };

            int changes = 0;

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
