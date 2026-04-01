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

namespace qon.QSL
{
    public static class Propagators
    {
        public static Result AllDistinctPropagator<TQ>(IEnumerable<QObject<TQ>> variables) where TQ : notnull
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
                changes += DomainLayer<TQ>.On(variable).RemoveValues(distinctVariables);
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

                    if (DomainLayer<TQ>.On(variable).IsEmpty())
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
    }
}
