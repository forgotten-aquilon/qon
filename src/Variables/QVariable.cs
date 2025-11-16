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
    public class QVariable<TQ> : ICopy<QVariable<TQ>>, ILayerHolder<TQ, QVariable<TQ>> where TQ : notnull
    {
        private QMachine<TQ>? _machine;

        public Guid Id { get; protected set; } = Guid.NewGuid();
        public string Name { get; protected set; }
        //TODO: Still not sure about general properties, when there are layers
        public Dictionary<string, ValueType> Properties { get; set; } = new Dictionary<string, ValueType>();
        public LayersManager<TQ, QVariable<TQ>> Layers { get; protected set; }
        public Optional<TQ> Value { get; set; } = Optional<TQ>.Empty;
        public ValueState State { get; set; } = ValueState.Uncertain;

        public QMachine<TQ> Machine
        {
            get => ExceptionHelper.ThrowIfInternalValueIsNull(_machine, nameof(Machine));
            set => _machine = value;
        }

        protected QVariable()
        {
            Layers = new LayersManager<TQ, QVariable<TQ>>(this);

            Name = "";
        }

        //TODO Rework with New/Empty
        public QVariable(string name)
        {
            Layers = new LayersManager<TQ, QVariable<TQ>>(this);

            if (string.IsNullOrEmpty(name))
            {
                Name = Id.ToString();

                return;
            }
            Name = name;
        }

        #region Methods

        public QVariable<TQ> AddProperty(string name, ValueType value)
        {
            AddNewProperty(name, value);
            return this;
        }

        public QVariable<TQ> WithValue(TQ value, ValueState state = ValueState.Constant)
        {
            Value = Optional<TQ>.Of(value);
            State = state;

            return this;
        }

        protected void AddNewProperty(string name, ValueType value)
        {
            if (Properties.ContainsKey(name))
            {
                throw new InternalLogicException("Property with this name already exists");
            }

            Properties[name] = value;
        }

        public virtual ValueType this[string propertyName]
        {
            get => Properties[propertyName];

            set => Properties[propertyName] = value;
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
                Layers = { Layers }
            };
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

        public static QVariable<TQ> Empty()
        {
            var newVariable = new QVariable<TQ>();
            newVariable.Name = newVariable.Id.ToString();
            return newVariable;
        }

        public static QVariable<TQ> Empty(string name)
        {
            QVariable<TQ> newVariable = new()
            {
                Name = name
            };
            return newVariable;
        }

        public static QVariable<TQ> New(TQ value, ValueState state = ValueState.Constant)
        {
            return Empty().WithValue(value, state);
        }

        public static QVariable<TQ> New(string name, TQ value, ValueState state = ValueState.Constant)
        {
            return Empty(name).WithValue(value, state);
        }
    }
}
