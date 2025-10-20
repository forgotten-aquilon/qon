using qon.Domains;
using qon.Functions;
using qon.Functions.Constraints;
using qon.Helpers;
using qon.Layers.VariableLayers;
using qon.Machines;
using qon.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qon.Layers.StateLayers
{
    public class ConstraintLayer<T> : BaseLayer<T, ConstraintLayer<T>, MachineState<T>>, ILayer<T, MachineState<T>>, IStateLayer<T>
    {
        public RuleHandler<T> Constraints { get; set; } = new();

        public ConstraintLayer()
        {

        }

        public ConstraintLayer(RuleHandler<T> constraints)
        {
            Constraints = constraints;
        }

        public static ConstraintLayer<T> TryCreate(MachineState<T> state, RuleHandler<T> constraints)
        {
            if (!state.Layers.TryGetLayer<ConstraintLayer<T>>(out var layer))
            {
                layer = new ConstraintLayer<T>(constraints);
                state.Layers.Add(layer);
            }

            return layer;
        }

        public static void Collapse(QVariable<T> variable, T value, bool isConstant = false)
        {
            variable.WithValue(value, isConstant ? ValueState.Constant : ValueState.Defined);
            DomainLayer<T>.With(variable).AssignEmptyDomain();
        }

        public static Optional<T> AutoCollapse(QVariable<T> variable)
        {
            var layer = DomainLayer<T>.With(variable);
            var value = layer.SingleOrEmptyValue();

            if (value.HasValue)
            {
                Collapse(variable, value.Value);
            }

            return value;
        }

        public Result Execute(QVariable<T>[] field)
        {
            int filterChanges = 0;

            int changes;
            do
            {
                changes = 0;

                var generalResult = ExecuteConstraints(Constraints.GeneralConstraints, field);

                if (generalResult.Failed)
                {
                    return generalResult;
                }

                changes += generalResult.ChangesAmount;
                changes += AutoCollapse(field);

                filterChanges += changes;
            }
            while (changes != 0);

            return Result.Success(filterChanges);
        }

        public bool Validate(QVariable<T>[] field)
        {
            if (Constraints.ValidationConstraints.IsNullOrEmpty())
            {
                return true;
            }

            return !ExecuteConstraints(Constraints.ValidationConstraints, field).Failed;
        }

        private static Result ExecuteConstraints(List<IQConstraint<T>> rules, QVariable<T>[] field)
        {
            int changes = 0;
            foreach (var rule in rules)
            {
                var result = rule.Execute(field);

                if (result.Failed)
                {
                    return result;
                }

                changes += result.ChangesAmount;
            }

            return Result.Success(changes);
        }

        public static int AutoCollapse(QVariable<T>[] field)
        {
            int changes = 0;

            foreach (var v in field)
            {
                if (v.State != ValueState.Uncertain)
                    continue;

                if (AutoCollapse(v).HasValue)
                {
                    changes++;
                }
            }

            return changes;
        }

        public ILayer<T, MachineState<T>> Copy()
        {
            throw new NotImplementedException();
        }
    }
}
