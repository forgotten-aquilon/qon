using qon.Domains;
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
    public class ConstraintLayer<T> : BaseLayer<T, ConstraintLayer<T>, MachineState<T>>, ILayer<T, MachineState<T>>
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
            DomainLayer<T>.With(variable).Domain = EmptyDomain<T>.Instance;
        }

        public static Optional<T> AutoCollapse(QVariable<T> variable)
        {
            var layer = DomainLayer<T>.With(variable);
            var value = layer.Domain.SingleOrEmptyValue();

            if (value.HasValue)
            {
                Collapse(variable, value.Value);
            }

            return value;
        }

        public static int AutoCollapse(MachineState<T> state)
        {
            int changes = 0;

            foreach (var v in state.Field)
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
