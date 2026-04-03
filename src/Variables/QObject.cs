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
using qon.QSL;

namespace qon.Variables
{
    /// <summary>
    /// Key class used for representation of variables for solving objectives
    /// </summary>
    /// <typeparam name="TQ">
    /// Key generic parameter
    /// </typeparam>
    public class QObject<TQ> : ICopy<QObject<TQ>>, ILayerHolder<TQ, QObject<TQ>>, IEquatable<QObject<TQ>> where TQ : notnull
    {
        public sealed class ValueChangedEventArgs : EventArgs
        {
            public Optional<TQ> PreviousValue { get; }

            public Optional<TQ> NewValue { get; }

            public ValueChangedEventArgs(Optional<TQ> previousValue, Optional<TQ> newValue)
            {
                PreviousValue = previousValue;
                NewValue = newValue;
            }
        }

        private Func<Guid>? _idResolver;

        /// <summary>
        /// Nullable reference to Solution Machine. Allows late binding to actual instance of machine.
        /// </summary>
        private QMachine<TQ>? _machine;

        private Optional<TQ> _value = Optional<TQ>.Empty;

        public Func<Guid> IdResolver 
        { 
            get => ExceptionHelper.ThrowIfFieldIsNull(_idResolver);
            set => _idResolver ??= value;
        }

        /// <summary>
        /// Unique ID, which is preserved by copying.
        /// </summary>
        public Guid Id => IdResolver();

        /// <summary>
        /// Human-defined name of the @object
        /// </summary>
        public string Name { get; protected set; }

        //NOTE: Still not sure about general properties, when layers exist
        /// <summary>
        /// Kinda obsolete way to storing additional data. Layers should be used instead
        /// </summary>
        public Dictionary<string, IConvertible> Properties { get; set; } = new Dictionary<string, IConvertible>();

        /// <summary>
        /// Contains layers with additional data or functionality
        /// </summary>
        public LayersManager<TQ, QObject<TQ>> LayerManager { get; protected set; }

        /// <summary>
        /// Actual value of the @object
        /// </summary>
        public Optional<TQ> Value
        {
            get => _value;
            set
            {
                if (_value == value)
                {
                    return;
                }

                var previousValue = _value;
                _value = value;

                ValueChanged?.Invoke(this, new ValueChangedEventArgs(previousValue, value));
            }
        }

        public event EventHandler<ValueChangedEventArgs>? ValueChanged;

        /// <summary>
        /// Non-nullable reference to Solution Machine, which is checked in runtime. Allows late binding to actual
        /// instance of machine.
        /// </summary>
        public QMachine<TQ> Machine
        {
            get => ExceptionHelper.ThrowIfInternalValueIsNull(_machine, nameof(Machine));
            set => _machine = value;
        }

        protected QObject()
        {
            LayerManager = new LayersManager<TQ, QObject<TQ>>(this);

            Name = "";
        }

        /// <summary>
        /// Chaining method for easy setup of Object's value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="state"></param>
        /// <returns>Instance of the same @object</returns>
        public QObject<TQ> WithValue(TQ value, ValueState state = ValueState.Constant)
        {
            Value = Optional<TQ>.Of(value);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual QObject<TQ> Copy()
        {
            return new QObject<TQ>()
            {
                IdResolver = IdResolver,
                Name = Name,
                Properties = new Dictionary<string, IConvertible>(Properties),
                Value = Value,
                Machine = Machine,
                LayerManager = { LayerManager }
            };
        }

        public QLink<TQ> ToLink()
        {
            return new QLink<TQ>((machine) => machine[this.Id], Machine);
        }

        /// <summary>
        /// Creates Object without value and with random ID and Name.
        /// </summary>
        /// <returns></returns>
        public static QObject<TQ> Empty()
        {
            var newId = Guid.NewGuid();
            var newVariable = new QObject<TQ>()
            {
                Name = newId.ToString()
            };

            return newVariable;
        }

        /// <summary>
        /// Creates Object without value with specified Name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static QObject<TQ> Empty(string name)
        {
            QObject<TQ> newObject = new()
            {
                Name = name,
            };
            return newObject;
        }

        /// <summary>
        /// Creates Object with specified Value and its State
        /// </summary>
        /// <param name="value"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static QObject<TQ> New(TQ value, ValueState state = ValueState.Constant)
        {
            return Empty().WithValue(value, state);
        }

        /// <summary>
        /// Creates Object with specified Name, Value and its State
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static QObject<TQ> New(string name, TQ value, ValueState state = ValueState.Constant)
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
        public QObject<TQ> AddProperty(string name, IConvertible value)
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
        public virtual IConvertible this[string propertyName]
        {
            get => Properties[propertyName];

            set => Properties[propertyName] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual object? GetNullOrValueProperty(string propertyName)
        {
            Properties.TryGetValue(propertyName, out IConvertible? property);
            return property;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool TryGetProperty(string propertyName, out IConvertible? property)
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

        #region IEquatable

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(QObject<TQ>? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            if (Id != other.Id)
                return false;

            if (Name != other.Name) 
                return false;

            if (Value != other.Value)
                return false;

            if (!Properties.SequenceEqual(other.Properties))
                return false;

            if (!LayerManager.Layers.SequenceEqual(other.LayerManager.Layers))
                return false;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((QObject<TQ>)obj);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return HashCode.Combine(_machine, Id, Name, Properties, LayerManager, Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(QObject<TQ> left, QObject<TQ> right)
        {
            return left.Equals(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(QObject<TQ> left, QObject<TQ> right)
        {
            return !(left == right);
        }

        #endregion
    }
}
