using qon.Functions;
using qon.Helpers;
using qon.Layers.VariableLayers;
using qon.Machines;
using qon.Variables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Exceptions;
using qon.Solvers;

namespace qon.Layers.StateLayers
{
    public class ConstraintLayerParameter<TQ> where TQ : notnull
    {
        public List<IPreparation<TQ>> GeneralConstraints { get; set; } = new List<IPreparation<TQ>>();
        public List<IPreparation<TQ>>? ValidationConstraints { get; set; } = new List<IPreparation<TQ>>();
    }

    public class ConstraintLayer<TQ> : BaseLayer<TQ, ConstraintLayer<TQ>, MachineState<TQ>>, ILayer<TQ, MachineState<TQ>>,
        IStateLayer<TQ> where TQ : notnull
    {
        public ConstraintLayerParameter<TQ> Constraints { get; set; } = new();

        public ConstraintLayer()
        {
        }

        public ConstraintLayer(ConstraintLayerParameter<TQ> constraints)
        {
            Constraints = constraints;
        }

        #region Solving lifecycle

        public Result Prepare(Field<TQ> field)
        {
            int filterChanges = 0;

            int changes;
            do
            {
                changes = 0;

                var generalResult = ExecuteConstraints(Constraints.GeneralConstraints, field, Machine);

                if (generalResult.Failed)
                {
                    return generalResult;
                }

                changes += generalResult.ChangesAmount;
                changes += AutoCollapse(field.Variables);

                filterChanges += changes;
            } while (changes != 0);

            return Result.Success(filterChanges);
        }

        public PreValidationResult PreValidate(Field<TQ> field)
        {
            foreach (var variable in field.Variables)
            {
                if (DomainLayer<TQ>.With(variable).IsEmpty() && variable.State == ValueState.Uncertain)
                    return PreValidationResult.InvalidState;

                if (variable.State == ValueState.Uncertain)
                    return PreValidationResult.NotValidated;
            }

            return PreValidationResult.PreValidated;
        }

        public bool Validate(Field<TQ> field)
        {
            if (Constraints.ValidationConstraints.IsNullOrEmpty())
            {
                return true;
            }

            return !ExecuteConstraints(Constraints.ValidationConstraints, field, null).Failed;
        }

        public void Execute(Field<TQ>? previousField, Field<TQ> currentField, Random random)
        {
            double entropy = double.MaxValue;
            QVariable<TQ>? candidate = null;

            foreach (QVariable<TQ> variable in currentField)
            {
                if (variable.State != ValueState.Uncertain)
                {
                    continue;
                }

                DomainLayer<TQ> domain = DomainLayer<TQ>.With(variable);
                double potentialEntropy = domain.Entropy;

                if (potentialEntropy < entropy)
                {
                    entropy = potentialEntropy;
                    candidate = variable;
                }
            }

            ExceptionHelper.ThrowIfInternalValueIsNull(candidate, nameof(candidate));

            DomainLayer<TQ> domainLayer = DomainLayer<TQ>.With(candidate);
            TQ value = domainLayer.GetRandomValue(random);

            Collapse(candidate, value);

            if (!previousField.IsNullOrEmpty())
            {
                QVariable<TQ>? previousCandidate = previousField.FirstOrDefault(v => v.Name == candidate.Name);

                ExceptionHelper.ThrowIfInternalValueIsNull(previousCandidate, nameof(previousCandidate));

                DomainLayer<TQ>.With(previousCandidate).RemoveValue(value);
            }
        }

        #endregion

        public static ConstraintLayer<TQ> TryCreate(MachineState<TQ> state, ConstraintLayerParameter<TQ> constraints)
        {
            if (!state.Layers.TryGetLayer<ConstraintLayer<TQ>>(out var layer))
            {
                layer = new ConstraintLayer<TQ>(constraints);
                state.Layers.Add(layer);
            }

            return layer;
        }

        public static void Collapse(QVariable<TQ> variable, TQ value, bool isConstant = false)
        {
            DomainLayer<TQ>.With(variable).Collapse(value, isConstant);
        }

        public static Optional<TQ> TryCollapseVariable(QVariable<TQ> variable)
        {
            var layer = DomainLayer<TQ>.With(variable);
            var value = layer.SingleOrEmptyValue();

            if (value.HasValue)
            {
                Collapse(variable, value.Value);
            }

            return value;
        }

        private static Result ExecuteConstraints(List<IPreparation<TQ>> rules, Field<TQ> field, QMachine<TQ>? machine)
        {
            int changes = 0;
            foreach (var rule in rules)
            {
                var result = rule.Execute(field, machine);

                if (result.Failed)
                {
                    return result;
                }

                changes += result.ChangesAmount;
            }

            return Result.Success(changes);
        }

        public static int AutoCollapse(QVariable<TQ>[] field)
        {
            int changes = 0;

            foreach (var v in field)
            {
                if (v.State != ValueState.Uncertain)
                    continue;

                if (TryCollapseVariable(v).HasValue)
                {
                    changes++;
                }
            }

            return changes;
        }

        public override ILayer<TQ, MachineState<TQ>> Copy()
        {
            throw new NotImplementedException();
        }
    }
}