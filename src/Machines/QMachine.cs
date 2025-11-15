using qon.Exceptions;
using qon.Helpers;
using qon.Layers.StateLayers;
using qon.Layers.VariableLayers;
using qon.Solvers;
using qon.Variables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using qon.Variables.Domains;

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
        protected Dictionary<string, int> _namedIndexer { get; set; } = new Dictionary<string, int>();
        protected Dictionary<Guid, int> _guidIndexer { get; set; } = new Dictionary<Guid, int>();
        public IReadOnlyDictionary<string, int> NamedIndexer => _namedIndexer;
        public IReadOnlyDictionary<Guid, int> GuidIndexer => _guidIndexer;

        public ISolver<T> Solver { get; protected set; }

        public States<T> States { get; protected set; }

        public MachineState<T> State { get; protected set; }

        public MachineStateType StateType {  get; set; }

        public Random Random { get; }

        public QVariable<T> this[string name] => State.Field[name];

        public QVariable<T> this[Guid id] => State.Field[id];

        //TODO: remove factory
        public QMachine(QMachineParameter<T> parameter)
        {
            State = new MachineState<T>(this);
            StateType = MachineStateType.Created;
            Solver = parameter.SolverInjection(this);
            States = new States<T>(this);
            Random = parameter.Random;

            if (parameter.Field is not null)
            {
                SetField(parameter.Field);
            }
        }

        public void SetField(IEnumerable<QVariable<T>> field)
        {
            State.SetField(field.ToArray());

            _namedIndexer.Clear();
            _guidIndexer.Clear();

            for (int i = 0; i < State.Field.Count; i++)
            {
                _namedIndexer[State.Field[i].Name] = i;
                _guidIndexer[State.Field[i].Id] = i;
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

        public void GenerateField(IDomain<T> domain, (int x, int y, int z) dimensions, Optional<T> defaultValue = new Optional<T>())
        {
            List<QVariable<T>> variables = new();

            if (dimensions.x < 1 || dimensions.y < 1 || dimensions.z < 1)
            {
                throw new InternalLogicException("Dimension can't be a non-positive number");
            }

            var layer = EuclideanStateLayer<T>.GetOrCreate(State);
            layer.FieldGrid = new string[dimensions.x, dimensions.y, dimensions.z];

            for (int x = 0; x < dimensions.x; x++)
            {
                for (int y = 0; y < dimensions.y; y++)
                {
                    for (int z = 0; z < dimensions.z; z++)
                    {
                        string name = $"{x}x{y}x{z}";
                        var v = new QVariable<T>(name);
                        DomainLayer<T>.GetOrCreate(v).AssignDomain(domain);
                        EuclideanLayer<T>.GetOrCreate(v).Update(x, y, z);

                        layer.FieldGrid[x, y, z] = name;

                        if (defaultValue.HasValue)
                        {
                            DomainLayer<T>.With(v).Collapse(defaultValue.Value, true);
                        }

                        variables.Add(v);
                    }
                }
            }

            SetField(variables);
        }
    }
}
