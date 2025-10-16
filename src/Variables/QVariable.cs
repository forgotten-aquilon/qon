using qon.Domains;
using qon.Exceptions;
using qon.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using qon.Variables.Layers;

namespace qon.Variables
{
    public class QVariable<T> : ICloneable
    {
        public Guid Id { get; protected set; } = Guid.NewGuid();
        public string Name { get; protected set; }
        public bool Protected { get; set; } = false;
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
        public LayersManager<T> Layers { get; protected set; } = new LayersManager<T>();
        public Optional<T> Value { get; protected set; } = Optional<T>.Empty;

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

        public QVariable(T value, string name = "") : this(name)
        {
            SuperpositionLayer<T>.Collapse(this, value, isConstant: true);
        }

        public QVariable<T> AddProperty(string name, object value)
        {
            AddNewProperty(name, value);
            return this;
        }

        public void UpdateValue(T value)
        {
            Value = Optional<T>.Of(value);
        }

        protected void AddNewProperty(string name, object value)
        {
            if (Properties.ContainsKey(name))
            {
                throw new InternalLogicException("Property with this name already exists");
            }

            Properties[name] = value;
        }

        public TOut? As<TOut>() where TOut : QVariable<T>
        {
            return this as TOut;
        }

        public virtual object this[string propertyName]
        {
            get => Properties[propertyName];

            set => Properties[propertyName] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual object Clone()
        {
            return Copy();
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
