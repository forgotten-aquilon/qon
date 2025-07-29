using qon.Domains;
using qon.Rules.Filters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace qon
{
    public enum MachineStateType
    {
        Created,
        Prepared,
        IsSolving,
        Validation,
        Finished,
        Error
    }

    public class States<T> : IEnumerable<MachineState<T>>
    {
        private readonly QMachine<T> _machine;

        public States(QMachine<T> machine)
        {
            _machine = machine;
        }

        public IEnumerator<MachineState<T>> GetEnumerator()
        {
            return _machine.Enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class QMachine<T>
    {
        public IEnumerator<MachineState<T>> Enumerator { get; protected set; }

        public States<T> States { get; protected set; }
        public RuleHandler<T> GeneralRules { get; set; }
        public RuleHandler<T>? ValidationRules { get; set; }
        public MachineState<T> State { get; protected set; }

        public MachineStateType StateType {  get; set; }

        public Random Random { get; }

        public QMachine(QMachineParameter<T> parameter)
        {
            GeneralRules = parameter.GeneralRules;
            ValidationRules = parameter.ValidationRules;

            State = new MachineState<T>();
            StateType = MachineStateType.Created;
            Enumerator = new Solver<T>(this);
            States = new States<T>(this);
            Random = parameter.Random;

            if (parameter.FieldParameter is not null)
            {
                if (parameter.FieldParameter.Field is null)
                {
                    throw new FieldNullException(nameof(parameter.FieldParameter.Field));
                }

                if (parameter.FieldParameter.Domain is null)
                {
                    throw new FieldNullException(nameof(parameter.FieldParameter.Domain));
                }

                foreach (var variable in parameter.FieldParameter.Field)
                {
                    variable.SetDomain(parameter.FieldParameter.Domain);
                }

                SetField(parameter.FieldParameter.Field);
            }
        }

        public virtual ConstraintResult ApplyConstraints(bool validation = false)
        {
            int filterChanges = 0;
            bool isConverged = true;

            int changes;
            do
            {
                changes = 0;

                var globalResult = State.ExecuteGlobalRules(GeneralRules.GlobalRules);

                if (globalResult.Outcome == PropagationOutcome.Conflict)
                {
                    return globalResult;
                }

                if (globalResult.Outcome == PropagationOutcome.UnderConstrained)
                {
                    isConverged = false;
                }

                changes += globalResult.ChangesAmount;
                changes += State.AutoCollapse();

                var localResult = State.ExecuteLocalRules(GeneralRules.LocalRules);

                if (localResult.Outcome == PropagationOutcome.Conflict)
                {
                    return localResult;
                }

                if (localResult.Outcome == PropagationOutcome.UnderConstrained)
                {
                    isConverged = false;
                }

                changes += localResult.ChangesAmount;
                changes += State.AutoCollapse();

                filterChanges += changes;
            }
            while (changes != 0);

            if (!validation)
            {
                return isConverged switch
                {
                    true => new ConstraintResult(PropagationOutcome.Converged, filterChanges),
                    false => new ConstraintResult(PropagationOutcome.UnderConstrained, filterChanges)
                };
            }

            var globalValidation = State.ExecuteGlobalRules(ValidationRules!.GlobalRules);

            if (globalValidation.Outcome == PropagationOutcome.Conflict)
            {
                return globalValidation;
            }

            var localValidation = State.ExecuteLocalRules(ValidationRules!.LocalRules);

            if (localValidation.Outcome == PropagationOutcome.Conflict)
            {
                return localValidation;
            }

            int validationChanges = globalValidation.ChangesAmount + localValidation.ChangesAmount;

            return new ConstraintResult(PropagationOutcome.Converged, validationChanges);
        }

        public void SetField(IEnumerable<SuperpositionVariable<T>> field)
        {
            State.Field = field.ToList();
            StateType = MachineStateType.Prepared;
        }

        public void SetState(MachineState<T> state)
        {
            State = state;
        }

        public void GenerateField(IDomain<T> d, int count)
        {
            var field = new List<SuperpositionVariable<T>>();
            for (int i = 0; i < count; i++)
            {
                field.Add(new SuperpositionVariable<T>(d));
            }
            SetField(field);
        }

        public void GenerateField(IDomain<T> d, IEnumerable<string> names)
        {
            var field = new List<SuperpositionVariable<T>>();
            foreach (var name in names)
            {
                field.Add(new SuperpositionVariable<T>(d, name));
            }
            SetField(field);
        }
    }
}
