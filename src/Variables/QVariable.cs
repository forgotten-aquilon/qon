using qon.Domains;
using qon.Exceptions;
using qon.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using qon.Layers;
using qon.Layers.VariableLayers;

namespace qon.Variables
{
    public class QVariable<T> : ICopy<QVariable<T>>, ILayerHolder<T, QVariable<T>>
    {
        public Guid Id { get; protected set; } = Guid.NewGuid();
        public string Name { get; protected set; }
        public bool Protected { get; set; } = false;
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
        public LayersManager<T, QVariable<T>> Layers { get; protected set; } = new LayersManager<T, QVariable<T>>();
        public Optional<T> Value { get; protected set; } = Optional<T>.Empty;
        public ValueState State { get; set; } = ValueState.Uncertain;

        protected QVariable()
        {
            Name = "";
        }

        public QVariable(string name)
        {
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
            return new QVariable<T>()
            {
                Id = Id,
                Name = Name,
                Properties = new Dictionary<string, object>(Properties),
                Value = Value,
                State = State,
                Layers = Layers.Copy()
            };
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
    }
}
