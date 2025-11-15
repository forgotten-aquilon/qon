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
    public class QVariable<T> : ICopy<QVariable<T>>, ILayerHolder<T, QVariable<T>>
    {
        private QMachine<T>? _machine;

        public Guid Id { get; protected set; } = Guid.NewGuid();
        public string Name { get; protected set; }
        public bool Protected { get; set; } = false;
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
        public LayersManager<T, QVariable<T>> Layers { get; protected set; }
        public Optional<T> Value { get; set; } = Optional<T>.Empty;
        public ValueState State { get; set; } = ValueState.Uncertain;

        public QMachine<T> Machine
        {
            get => ExceptionHelper.ThrowIfInternalValueIsNull(_machine, nameof(Machine));
            set => _machine = value;
        }

        protected QVariable()
        {
            Layers = new LayersManager<T, QVariable<T>>(this);

            Name = "";
        }

        //TODO Rework with New/Empty
        public QVariable(string name)
        {
            Layers = new LayersManager<T, QVariable<T>>(this);

            if (string.IsNullOrEmpty(name))
            {
                Name = Id.ToString();

                return;
            }
            Name = name;
        }

        public QVariable<T> AddProperty(string name, object value)
        {
            AddNewProperty(name, value);
            return this;
        }

        public QVariable<T> WithValue(T value, ValueState state = ValueState.Constant)
        {
            Value = Optional<T>.Of(value);
            State = state;

            return this;
        }

        protected void AddNewProperty(string name, object value)
        {
            if (Properties.ContainsKey(name))
            {
                throw new InternalLogicException("Property with this name already exists");
            }

            Properties[name] = value;
        }

        public virtual object this[string propertyName]
        {
            get => Properties[propertyName];

            set => Properties[propertyName] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual QVariable<T> Copy()
        {
            var result = new QVariable<T>()
            {
                Id = Id,
                Name = Name,
                Properties = new Dictionary<string, object>(Properties),
                Value = Value,
                State = State,
                Machine = Machine
            };

            result.Layers = Layers.Copy(result);

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual object? GetNullOrValueProperty(string propertyName)
        {
            Properties.TryGetValue(propertyName, out object? property);
            return property;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool TryGetProperty(string propertyName, out object? property)
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

        public static QVariable<T> New()
        {
            var newVariable = new QVariable<T>();
            newVariable.Name = newVariable.Id.ToString();
            return newVariable;
        }

        public static QVariable<T> New(T value, ValueState state = ValueState.Constant)
        {
            return New().WithValue(value, state);
        }
    }
}
