using qon.Exceptions;
using qon.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using qon.Layers;
using qon.Layers.VariableLayers;
using qon.Machines;

namespace qon.Variables
{
    /// <summary>
    /// Key class used for representation of variables for solving objectives
    /// </summary>
    /// <typeparam name="TQ">
    /// Key generic parameter
    /// </typeparam>
    public class QVariable<TQ> : ICopy<QVariable<TQ>>, ILayerHolder<TQ, QVariable<TQ>> where TQ : notnull
    {
        /// <summary>
        /// Nullable reference to Solution Machine. Allows late binding to actual instance of machine.
        /// </summary>
        private QMachine<TQ>? _machine;

        /// <summary>
        /// Unique ID, which is preserved by copying.
        /// </summary>
        public Guid Id { get; protected set; } = Guid.NewGuid(); // TODO: Remove unnecessary initialization

        /// <summary>
        /// Human-defined name of the variable, there is no check for its uniqueness, but it should be.
        /// </summary>
        public string Name { get; protected set; } //TODO: Add uniqueness check in <see cref="QMachine{TQ}"/>

        //TODO: Still not sure about general properties, when there are layers
        /// <summary>
        /// Kinda obsolete way to storing additional data. Layers should be used instead
        /// </summary>
        public Dictionary<string, ValueType> Properties { get; set; } = new Dictionary<string, ValueType>();

        /// <summary>
        /// Contains layers with additional data or functionality
        /// </summary>
        public LayersManager<TQ, QVariable<TQ>> LayerManager { get; protected set; }

        /// <summary>
        /// Actual value of the variable
        /// </summary>
        public Optional<TQ> Value { get; set; } = Optional<TQ>.Empty;

        /// <summary>
        /// State of the variable
        /// </summary>
        public ValueState State { get; set; } = ValueState.Uncertain;

        /// <summary>
        /// Non-nullable reference to Solution Machine, which is checked in runtime. Allows late binding to actual
        /// instance of machine.
        /// </summary>
        public QMachine<TQ> Machine
        {
            get => ExceptionHelper.ThrowIfInternalValueIsNull(_machine, nameof(Machine));
            set => _machine = value;
        }

        protected QVariable()
        {
            LayerManager = new LayersManager<TQ, QVariable<TQ>>(this);

            Name = "";
        }

        /// <summary>
        /// Chaining method for easy setup of Variable's value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="state"></param>
        /// <returns>Instance of the same variable</returns>
        public QVariable<TQ> WithValue(TQ value, ValueState state = ValueState.Constant)
        {
            Value = Optional<TQ>.Of(value);
            State = state;

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual QVariable<TQ> Copy()
        {
            return new QVariable<TQ>()
            {
                Id = Id,
                Name = Name,
                Properties = new Dictionary<string, ValueType>(Properties),
                Value = Value,
                State = State,
                Machine = Machine,
                LayerManager = { LayerManager }
            };
        }

        /// <summary>
        /// Creates Variable without value and with random ID and Name.
        /// </summary>
        /// <returns></returns>
        public static QVariable<TQ> Empty()
        {
            var newVariable = new QVariable<TQ>();
            newVariable.Name = newVariable.Id.ToString();
            return newVariable;
        }

        /// <summary>
        /// Creates Variable without value with specified Name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static QVariable<TQ> Empty(string name)
        {
            QVariable<TQ> newVariable = new()
            {
                Name = name
            };
            return newVariable;
        }

        /// <summary>
        /// Creates Variable with specified Value and its State
        /// </summary>
        /// <param name="value"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static QVariable<TQ> New(TQ value, ValueState state = ValueState.Constant)
        {
            return Empty().WithValue(value, state);
        }

        /// <summary>
        /// Creates Variable with specified Name, Value and its State
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static QVariable<TQ> New(string name, TQ value, ValueState state = ValueState.Constant)
        {
            return Empty(name).WithValue(value, state);
        }

        #region Property's methods

        /// <summary>
        /// Adds new property
        /// </summary>
        /// <param name="name">Property's name</param>
        /// <param name="value">Value</param>
        /// <returns></returns>
        public QVariable<TQ> AddProperty(string name, ValueType value)
        {
            if (!Properties.TryAdd(name, value))
            {
                throw new InternalLogicException("Property with this name already exists");
            }

            return this;
        }
        
        /// <summary>
        /// Easy access to properties without reaching the dictionary
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public virtual ValueType this[string propertyName]
        {
            get => Properties[propertyName];

            set => Properties[propertyName] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual object? GetNullOrValueProperty(string propertyName)
        {
            Properties.TryGetValue(propertyName, out ValueType? property);
            return property;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool TryGetProperty(string propertyName, out ValueType? property)
        {
            return Properties.TryGetValue(propertyName, out property);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool ContainsProperty(string propertyName)
        {
            return Properties.ContainsKey(propertyName);
        }

        public override string ToString()
        {
            return $"{Name.ToShortString(5)}:{Value}";
        }

        #endregion
    }
}
