using qon.Domains;
using qon.Rules.Filters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using qon.Solvers;
using qon.Variables;
using qon.Exceptions;

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
        protected Dictionary<string, int> _indexer { get; set; } = new Dictionary<string, int>();
        public IReadOnlyDictionary<string, int> Indexer => _indexer;
        
        public IEnumerator<MachineState<T>> Enumerator { get; protected set; }

        public States<T> States { get; protected set; }
        public RuleHandler<T> GeneralRules { get; set; }
        public RuleHandler<T>? ValidationRules { get; set; }
        public MachineState<T> State { get; protected set; }

        public MachineStateType StateType {  get; set; }

        public Random Random { get; }

        public SuperpositionVariable<T> this[string name]
        {
            get
            {
                return State.Field[_indexer[name]];
            }
        }

        public QMachine(QMachineParameter<T> parameter)
        {
            GeneralRules = parameter.GeneralRules;
            ValidationRules = parameter.ValidationRules;

            State = new MachineState<T>();
            StateType = MachineStateType.Created;
            Enumerator = new FiniteSolver<T>(this);
            States = new States<T>(this);
            Random = parameter.Random;

            if (parameter.FieldParameter is not null)
            {
                ExceptionHelper.ThrowIfFieldIsNull(parameter.FieldParameter.Field, nameof(parameter.FieldParameter.Field));

                ExceptionHelper.ThrowIfFieldIsNull(parameter.FieldParameter.Domain, nameof(parameter.FieldParameter.Domain));

                foreach (var variable in parameter.FieldParameter.Field)
                {
                    variable.SetDomain(parameter.FieldParameter.Domain);
                }

                SetField(parameter.FieldParameter.Field);
            }
        }

        public void SetField(IEnumerable<SuperpositionVariable<T>> field)
        {
            State.SetField(field.ToList());

            _indexer.Clear();

            for (int i = 0; i < State.Field.Count; i++)
            {
                _indexer[State.Field[i].Name] = i;
            }

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
