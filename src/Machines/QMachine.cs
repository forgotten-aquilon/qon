using qon.Exceptions;
using qon.Helpers;
using qon.Layers;
using qon.Layers.StateLayers;
using qon.Layers.VariableLayers;
using qon.Solvers;
using qon.Variables;
using qon.Variables.Domains;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace qon.Machines
{
    /// <summary>
    /// Specifies possible states of a machine during its lifecycle.
    /// </summary>
    public enum MachineStateType
    {
        Created,
        Ready,
        IsSolving,
        Validation,
        Finished,
        Error
    }

    public class States<TQ> : IEnumerable<MachineState<TQ>> where TQ : notnull
    {
        private readonly QMachine<TQ> _machine;

        public States(QMachine<TQ> machine)
        {
            _machine = machine;
        }

        public IEnumerator<MachineState<TQ>> GetEnumerator()
        {
            return _machine.Solver;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    /// <summary>
    /// Represents a machine that utilizes a solver to find solutions for a set of variables.
    /// </summary>
    /// <typeparam name="TQ">Key generic parameter</typeparam>
    public class QMachine<TQ> where TQ : notnull
    {
        protected Dictionary<string, int> _namedIndexer { get; set; } = new Dictionary<string, int>();
        protected Dictionary<Guid, int> _guidIndexer { get; set; } = new Dictionary<Guid, int>();
        /// <summary>
        /// Property which maps @object names to their respective indices in the machine's field.
        /// </summary>
        public IReadOnlyDictionary<string, int> NamedIndexer => _namedIndexer;
        /// <summary>
        /// Property which maps @object ids to their respective indices in the machine's field.
        /// </summary>
        public IReadOnlyDictionary<Guid, int> GuidIndexer => _guidIndexer;

        /// <summary>
        /// Property representing the solver used by the machine.
        /// </summary>
        public ISolver<TQ> Solver { get; protected set; }

        /// <summary>
        /// Property allowing enumeration over the states of the machine.
        /// </summary>
        public States<TQ> States { get; protected set; }

        /// <summary>
        /// Property representing the current <see cref="MachineState{TQ}"/> of the machine.
        /// </summary>
        public MachineState<TQ> State { get; protected set; }

        /// <summary>
        /// Property representing the current <see cref="MachineStateType"/> of the machine.
        /// </summary>
        public MachineStateType Status {  get; set; }

        /// <summary>
        /// Single point of access to random number generator instance.
        /// </summary>
        public Random Random { get; }

        /// <param name="name"></param>
        /// <returns>Object from the current <see cref="MachineState{TQ}"/></returns>
        public QObject<TQ> this[string name] => State.Field[name];

        /// <param name="id"></param>
        /// <returns>Object from the current <see cref="MachineState{TQ}"/></returns>
        public QObject<TQ> this[Guid id] => State.Field[id];

        public QMachine(QMachineParameter<TQ> parameter)
        {
            Random = parameter.Random;
            Status = MachineStateType.Created;
            Solver = parameter.SolverInit(this);
            State = new MachineState<TQ>(this);
            States = new States<TQ>(this);
        }

        public void AddToField(QObject<TQ> @object)
        {
            int newPos = State.AddToField(@object);

            _namedIndexer.Add(@object.Name, newPos);

            Guid id = Guid.NewGuid();

            _guidIndexer.Add(id, newPos);

            @object.Machine = this;
            @object.IdResolver = () => id;


            Status = MachineStateType.Ready;
        }

        public QLink<TQ> Q(string? name = null)
        {
            var q = name.IsNullOrEmpty() ? QObject<TQ>.Empty() : QObject<TQ>.Empty(name);

            AddToField(q);

            return q.ToLink();
        }

        public void Clear()
        {
            //TODO: Add clearing functions
        }
    }
}
