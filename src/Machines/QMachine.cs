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
        /// Property which maps variable names to their respective indices in the machine's field.
        /// </summary>
        public IReadOnlyDictionary<string, int> NamedIndexer => _namedIndexer;
        /// <summary>
        /// Property which maps variable ids to their respective indices in the machine's field.
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
        public MachineStateType StateType {  get; set; }

        /// <summary>
        /// Single point of access to random number generator instance.
        /// </summary>
        public Random Random { get; }

        /// <param name="name"></param>
        /// <returns>Variable from the current <see cref="MachineState{TQ}"/></returns>
        public QVariable<TQ> this[string name] => State.Field[name];

        /// <param name="id"></param>
        /// <returns>Variable from the current <see cref="MachineState{TQ}"/></returns>
        public QVariable<TQ> this[Guid id] => State.Field[id];

        public QMachine(QMachineParameter<TQ> parameter)
        {
            State = new MachineState<TQ>(this);
            StateType = MachineStateType.Created;
            Solver = parameter.SolverInjection(this);
            States = new States<TQ>(this);
            Random = parameter.Random;

            if (parameter.Field is not null)
            {
                InitializeField(parameter.Field);
            }
        }

        /// <summary>
        /// Sets <see cref="Field{TQ}"/> of the current <see cref="MachineState{TQ}"/> and binds all variables with this <see cref="QMachine{TQ}"/>
        /// </summary>
        /// <param name="field"></param>
        public void InitializeField(IEnumerable<QVariable<TQ>> field)
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
    }
}
