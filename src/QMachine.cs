using qon.Domains;
using qon.Rules;
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
        public List<IGlobalRule<T>> GlobalRules { get; set; }
        public List<ILocalRule<T>> LocalRules { get; set; }

        public MachineState<T> State { get; protected set; }

        public MachineStateType StateType {  get; set; }

        public Random Random { get; }

        public QMachine(QMachineParameter<T> parameter)
        {
            GlobalRules = parameter.GlobalRules;
            LocalRules = parameter.VariableRules;
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

        public virtual ConstraintResult ApplyConstraints()
        {
            int filterChanges = 0;

            int changes;
            do
            {
                changes = 0;

                var globalResult = State.ExecuteGlobalRules(GlobalRules);

                if (globalResult.Outcome == PropagationOutcome.Conflict)
                {
                    return globalResult;
                }

                changes += globalResult.ChangesAmount;
                changes += State.AutoCollapse();

                State.ExecuteLocalRules(LocalRules);

                filterChanges += changes;
            }
            while (changes != 0);

            return new ConstraintResult { ChangesAmount = filterChanges };
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
