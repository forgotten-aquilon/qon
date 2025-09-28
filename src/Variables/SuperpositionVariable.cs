using qon.Domains;
using qon.Exceptions;
using qon.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace qon.Variables
{
    public enum SuperpositionState
    {
        Constant,
        Uncertain,
        Defined
    }

    public class SuperpositionVariable<T> : ICloneable
    {
        protected Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        public string Name
        {
            get 
            {
                return Properties["@Name"] as string ?? "";
            } 
            protected set
            {
                Properties["@Name"] = value;
            }
        }

        public IDomain<T> Domain { get; set; }
        public SuperpositionState State { get; protected set; }
        public Optional<T> Value { get; protected set; } = Optional<T>.Empty;

        public double Entropy
        {
            get 
            {
                return Domain.GetEntropy();
            }
        }

        protected SuperpositionVariable()
        {
            Domain = new DiscreteDomain<T>();
            Name = string.Empty;
        }

        protected SuperpositionVariable(string name)
        {
            Name = string.IsNullOrEmpty(name) ? Guid.NewGuid().ToString() : name;
            Domain = new DiscreteDomain<T>();
        }

        public SuperpositionVariable(T value, string name = "") : this(name)
        {
            Value = new Optional<T>(value);

            State = Value.HasValue ? SuperpositionState.Constant : SuperpositionState.Uncertain;
            Domain = new DiscreteDomain<T>();
        }

        public SuperpositionVariable(IDomain<T> domain, string name = "") : this(name) 
        {
            ExceptionHelper.ThrowIfArgumentIsNull(domain, nameof(domain));

            State = SuperpositionState.Uncertain;
            Domain = domain.Copy();
        }
 
        public SuperpositionVariable(IDomain<T> domain, T value, string name = "") : this(domain, name)
        {
            Value = new Optional<T>(value);

            if (Value.HasValue)
            {
                State = SuperpositionState.Constant;
            }
        }

        //TODO: Properly implement
        public SuperpositionVariable<T> AddProperty(string name, object value)
        {
            if (Properties.ContainsKey(name))
            {
                throw new InternalLogicException("Property with this name already exists");
            }

            Properties[name] = value;
            return this;
        }

        public int SetDomain(IDomain<T> domain)
        {
            if (State != SuperpositionState.Uncertain)
            {
                Domain = EmptyDomain<T>.Instance;
            }
            else
            {
                Domain = domain.Copy();
            }

            return Domain.Size();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int RemoveFromDomain(IEnumerable<T> items)
        {
            return Domain.Remove(items);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int RemoveFromDomain(T item)
        {
            return Domain.Remove(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Collapse(T value, bool isConstant = false)
        {
            State = isConstant switch
            {
                true => SuperpositionState.Constant,
                false => SuperpositionState.Defined,
            };

            Value = new Optional<T>(value);

            Domain = EmptyDomain<T>.Instance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Optional<T> AutoCollapse()
        {
            var value = Domain.SingleOrEmptyValue();

            if (value.HasValue)
            {
                Collapse(value.Value);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual object Clone()
        {
            SuperpositionVariable<T> clone = new()
            {
                Domain = Domain.Copy(),
                Properties = new Dictionary<string, object>(Properties),
                Value = Value.Copy(),
                State = State,
                Name = Name,
            };

            return clone;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SuperpositionVariable<T> Copy()
        {
            return (SuperpositionVariable<T>)Clone();
        }

        public virtual object this[string propertyName]
        {
            get
            {
                return Properties[propertyName];
            }

            set 
            { 
                Properties[propertyName] = value; 
            }
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
