using qon.Domains;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using qon.Solvers;
using qon.Variables;
using qon.Variables.Layers;
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
        public RuleHandler<T> Constraints { get; set; }
        public MachineState<T> State { get; protected set; }

        public MachineStateType StateType {  get; set; }

        public Random Random { get; }

        public QVariable<T> this[string name]
        {
            get
            {
                return State.Field[_indexer[name]];
            }
        }

        public QMachine(QMachineParameter<T> parameter)
        {
            Constraints = parameter.Constraints;

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
                    SuperpositionLayer<T>.With(variable).Domain = parameter.FieldParameter.Domain;
                }

                SetField(parameter.FieldParameter.Field);
            }
        }

        public void SetField(IEnumerable<QVariable<T>> field)
        {
            State.SetField(field.ToArray());

            _indexer.Clear();

            for (int i = 0; i < State.Field.Length; i++)
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
            var field = new List<QVariable<T>>();
            for (int i = 0; i < count; i++)
            {
                var variable = new QVariable<T>(string.Empty);
                SuperpositionLayer<T>.For(variable).Domain = d;
                field.Add(variable);
            }
            SetField(field);
        }

        public void GenerateField(IDomain<T> d, IEnumerable<string> names)
        {
            var field = new List<QVariable<T>>();
            foreach (var name in names)
            {
                var variable = new QVariable<T>(name);
                SuperpositionLayer<T>.For(variable).Domain = d;
                field.Add(variable);
            }
            SetField(field);
        }
    }
}
