using qon.Domains;
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

        #region Solving lifecycle

        public Result Prepare(Field<T> field)
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
            }
            while (changes != 0);

            return Result.Success(filterChanges);
        }

        public PreValidationResult PreValidate(Field<T> field)
        {
            foreach (var variable in field.Variables)
            {
                if (DomainLayer<T>.With(variable).IsEmpty() && variable.State == ValueState.Uncertain)
                    return PreValidationResult.InvalidState;

                if (variable.State == ValueState.Uncertain)
                    return PreValidationResult.NotValidated;
            }

            return PreValidationResult.PreValidated;
        }

        public bool Validate(Field<T> field)
        {
            if (Constraints.ValidationConstraints.IsNullOrEmpty())
            {
                return true;
            }

            return !ExecuteConstraints(Constraints.ValidationConstraints, field, null).Failed;
        }

        public void Execute(Field<T>? previousField, Field<T> currentField, Random random)
        {
            double entropy = double.MaxValue;
            QVariable<T>? candidate = null;

            foreach (QVariable<T> variable in currentField)
            {
                if (variable.State != ValueState.Uncertain)
                {
                    continue;
                }

                DomainLayer<T> domain = DomainLayer<T>.With(variable);
                double potentialEntropy = domain.Entropy;

                if (potentialEntropy < entropy)
                {
                    entropy = potentialEntropy;
                    candidate = variable;
                }
            }

            ExceptionHelper.ThrowIfInternalValueIsNull(candidate, nameof(candidate));

            DomainLayer<T> domainLayer = DomainLayer<T>.With(candidate);
            T value = domainLayer.GetRandomValue(random);

            Collapse(candidate, value);

            if (!previousField.IsNullOrEmpty())
            {
                QVariable<T>? previousCandidate = previousField.FirstOrDefault(v => v.Name == candidate.Name);

                ExceptionHelper.ThrowIfInternalValueIsNull(previousCandidate, nameof(previousCandidate));

                DomainLayer<T>.With(previousCandidate).RemoveValue(value);
            }
        }

        #endregion

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

        public static Optional<T> TryCollapseVariable(QVariable<T> variable)
        {
            var layer = DomainLayer<T>.With(variable);
            var value = layer.SingleOrEmptyValue();

            if (value.HasValue)
            {
                Collapse(variable, value.Value);
            }

            return value;
        }

        private static Result ExecuteConstraints(List<IPreparation<T>> rules, Field<T> field, QMachine<T>? machine)
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

        public static int AutoCollapse(QVariable<T>[] field)
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

        public ILayer<T, MachineState<T>> Copy()
        {
            throw new NotImplementedException();
        }
    }
}
