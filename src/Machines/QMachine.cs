using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using qon.Domains;
using qon.Exceptions;
using qon.Layers.VariableLayers;
using qon.Variables;

namespace qon.Machines
{
    public enum MachineStateType
    {
        Created,
        Ready,
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
            return _machine.Solver;
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
        
        public IEnumerator<MachineState<T>> Solver { get; protected set; }

        public States<T> States { get; protected set; }
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

        public QMachine(QMachineParameter<T> parameter, Func<QMachine<T>, IEnumerator<MachineState<T>>> factory)
        {
            State = new MachineState<T>(this);
            StateType = MachineStateType.Created;
            Solver = factory(this);
            States = new States<T>(this);
            Random = parameter.Random;

            if (parameter.Field is not null)
            {
                ExceptionHelper.ThrowIfFieldIsNull(parameter.Field, nameof(parameter.Field));
                SetField(parameter.Field);
            }
        }

        public void SetField(IEnumerable<QVariable<T>> field)
        {
            State.SetField(field.ToArray());

            _indexer.Clear();

            for (int i = 0; i < State.Field.Length; i++)
            {
                _indexer[State.Field[i].Name] = i;
                State.Field[i].Machine = this;
            }

            StateType = MachineStateType.Ready;
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
                DomainLayer<T>.GetOrCreate(variable).AssignDomain(d);
                DomainLayer<T>.GetOrCreate(variable).AssignDomain(d);
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
                DomainLayer<T>.GetOrCreate(variable).AssignDomain(d);
                field.Add(variable);
            }
            SetField(field);
        }
    }
}
