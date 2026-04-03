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
using qon.QSL;
using qon.Solvers;

namespace qon.Layers.StateLayers
{
    public class ConstraintLayerParameter<TQ> where TQ : notnull
    {
        public List<IPreparation<TQ>> GeneralConstraints { get; set; } = new List<IPreparation<TQ>>();
        public List<IPreparation<TQ>> ValidationConstraints { get; set; } = new List<IPreparation<TQ>>();
    }


    public class ConstraintLayer<TQ> : 
        BaseLayer<TQ, ConstraintLayer<TQ>, MachineState<TQ>>, 
        ILayer<TQ, MachineState<TQ>>,
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

                var generalResult = ExecuteConstraints(Constraints.GeneralConstraints, field);

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
                if (variable.OnDomainLayer().IsEmpty() && variable.OnDomainLayer().State == ValueState.Uncertain)
                    return PreValidationResult.InvalidState;

                if (variable.OnDomainLayer().State == ValueState.Uncertain)
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

            return !ExecuteConstraints(Constraints.ValidationConstraints, field).Failed;
        }

        public void Execute(Field<TQ>? previousField, Field<TQ> currentField)
        {
            double entropy = double.MaxValue;
            QObject<TQ>? candidate = null;

            foreach (QObject<TQ> variable in currentField)
            {
                if (variable.OnDomainLayer().State != ValueState.Uncertain)
                {
                    continue;
                }

                //TODO: add caching for domain entropy
                DomainLayer<TQ> domain = DomainLayer<TQ>.On(variable);
                double potentialEntropy = domain.Entropy;

                if (potentialEntropy < entropy)
                {
                    entropy = potentialEntropy;
                    candidate = variable;
                }
            }

            ExceptionHelper.ThrowIfInternalValueIsNull(candidate, nameof(candidate));

            DomainLayer<TQ> domainLayer = DomainLayer<TQ>.On(candidate);
            TQ value = domainLayer.GetRandomValue(currentField.Machine.Random);

            candidate.Value = value;

            if (!previousField.IsNullOrEmpty())
            {
                QObject<TQ> previousCandidate = previousField[candidate.Name];

                DomainLayer<TQ>.On(previousCandidate).RemoveValue(value);
            }
        }

        #endregion

        public static ConstraintLayer<TQ> TryCreate(MachineState<TQ> state, ConstraintLayerParameter<TQ> constraints)
        {
            if (!state.LayerManager.TryGetLayer<ConstraintLayer<TQ>>(out var layer))
            {
                layer = new ConstraintLayer<TQ>(constraints);
                state.LayerManager.Add(layer);
            }

            return layer;
        }


        public static Optional<TQ> TryCollapseVariable(QObject<TQ> @object)
        {
            var layer = DomainLayer<TQ>.On(@object);
            var value = layer.SingleOrEmptyValue();

            if (value.HasValue)
            {
                @object.Value = value;
            }

            return value;
        }

        private static Result ExecuteConstraints(List<IPreparation<TQ>> rules, Field<TQ> field)
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

        public static int AutoCollapse(QObject<TQ>[] field)
        {
            int changes = 0;

            foreach (var variable in field)
            {
                if (variable.OnDomainLayer().State != ValueState.Uncertain)
                    continue;

                if (TryCollapseVariable(variable).HasValue)
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