using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using qon.Domains;

namespace qon
{
    public enum SuperpositionState
    {
        Constant,
        Uncertain,
        Defined
    }

    public class SuperpositionVariable<T> : ICloneable
    {
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
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
        public SuperpositionState State { get; private set; }
        public Optional<T> Value { get; private set; } = Optional<T>.Empty;

        public double Entropy
        {
            get 
            {
                return Domain.GetEntropy();
            }
        }

        private SuperpositionVariable()
        {
            Domain = new DiscreteDomain<T>();
            Name = string.Empty;
        }

        private SuperpositionVariable(string name)
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

        public SuperpositionVariable(IDomain<T>? domain, string name = "") : this(name) 
        {
            if (domain is null)
            {
                throw new FieldNullException(nameof(domain));
            }

            State = SuperpositionState.Uncertain;
            Domain = domain.Copy();
        }
 
        public SuperpositionVariable(IDomain<T>? domain, T value, string name = "") : this(domain, name)
        {
            Value = new Optional<T>(value);

            if (Value.HasValue)
            {
                State = SuperpositionState.Constant;
            }
        }

        //TODO: Properly implement
        public SuperpositionVariable<T> WithProperty(string name, object value)
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
                Domain = domain;
            }

            return Domain.Size();
        }

        public int RemoveFromDomain(IEnumerable<T> items)
        {
            return Domain.Remove(items);
        }


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

        public Optional<T> AutoCollapse()
        {
            if (Domain.Size() == 1)
            {
                Collapse(Domain.GetIEnumerable().FirstOrDefault().Key, false);

                return Value;
            }

            return Optional<T>.Empty;
        }

        public object Clone()
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

        public SuperpositionVariable<T> Copy()
        {
            return (SuperpositionVariable<T>)Clone();
        }
    }
}
