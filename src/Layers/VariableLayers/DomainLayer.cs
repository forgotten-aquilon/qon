using qon.Exceptions;
using qon.Helpers;
using qon.Machines;
using qon.Variables;
using qon.Variables.Domains;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using qon.Variables.Events;

namespace qon.Layers.VariableLayers
{
    public class DomainLayer<TQ> : BaseLayer<TQ, DomainLayer<TQ>, QObject<TQ>>, ILayer<TQ, QObject<TQ>> where TQ : notnull
    {
        private IDomain<TQ> _domain;

        public ValueState State { get; set; } = ValueState.Uncertain;

        public IDomain<TQ> Domain
        {
            get => _domain;
            //Can't wait to use 'field' keyword when Unity finally switches to CoreCLR
            //[MemberNotNull(nameof(_domain))]
            set => _domain = value.Copy();
        }

        public double Entropy => Domain.GetEntropy();

        public bool IsEmpty()
        {
            return Domain.IsEmpty();
        }

        public bool ContainsValue(TQ value)
        {
            return Domain.ContainsValue(value);
        }

        public int Size()
        {
            return Domain.Size();
        }

        public int RemoveValue(TQ value)
        {
            return Domain.Remove(value);
        }

        public int RemoveValues(IEnumerable<TQ> values)
        {
            ExceptionHelper.ThrowIfArgumentIsNull(values, nameof(values));
            return Domain.Remove(values);
        }

        public void AssignDomain(IDomain<TQ> domain)
        {
            Domain = domain;
        }

        public void AssignEmptyDomain()
        {
            Domain = EmptyDomain<TQ>.Instance;
        }

        public TQ GetRandomValue(Random random)
        {
            ExceptionHelper.ThrowIfArgumentIsNull(random, nameof(random));
            return Domain.GetRandomValue(random);
        }

        public Optional<TQ> SingleOrEmptyValue()
        {
            return Domain.SingleOrEmptyValue();
        }

        //public bool MatchesDomain(Func<IDomain<TQ>, bool> predicate)
        //{
        //    ExceptionHelper.ThrowIfArgumentIsNull(predicate, nameof(predicate));
        //    return predicate(Domain);
        //}

        //public bool DomainSizeSatisfies(Func<int, bool> predicate)
        //{
        //    ExceptionHelper.ThrowIfArgumentIsNull(predicate, nameof(predicate));
        //    return predicate(Size());
        //}

        public TResult WithDomain<TResult>(Func<IDomain<TQ>, TResult> selector)
        {
            ExceptionHelper.ThrowIfArgumentIsNull(selector, nameof(selector));
            return selector(Domain);
        }

        public string DescribeDomain()
        {
            return Domain.ToString() ?? string.Empty;
        }

        public IDomain<TQ> GetDomain()
        {
            return Domain;
        }

        public bool TryGetDomain<TDomain>([NotNullWhen(true)] out TDomain? domain) where TDomain : class, IDomain<TQ>
        {
            if (Domain is TDomain typed)
            {
                domain = typed;
                return true;
            }

            domain = null;
            return false;
        }

        public DomainLayer()
            : this(EmptyDomain<TQ>.Instance)
        {
        }

        public DomainLayer(IDomain<TQ> domain)
        {
            _domain = domain;
        }

        #region Overrides of BaseLayer<TQ,DomainLayer<TQ>,QObject<TQ>>

        public override ILayer<TQ, QObject<TQ>> Copy()
        {
            return new DomainLayer<TQ>(Domain.Copy())
            {
                NullableManager = NullableManager,
                State = State
            };
        }

        public override void AttachManager(LayersManager<TQ, QObject<TQ>> manager)
        {
            base.AttachManager(manager);

            Holder.ValueChanged += OnHolderValueChanged;
        }

        #endregion

        public override bool Equals(ILayer<TQ, QObject<TQ>> other)
        {
            if (base.Equals(other))
            {
                return true;
            }

            if (other is not DomainLayer<TQ> otherLayer)
            {
                return false;
            }

            if (otherLayer.State != State)
            {
                return false;
            }

            return Domain.Equals(otherLayer.Domain);
        }

        private void OnHolderValueChanged(object? sender, ValueChangedEventArgs<Optional<TQ>> e)
        {
            if (e.NewValue == Optional<TQ>.Empty)
            {
                State = ValueState.Uncertain;
                return;
            }

            State = Holder.Machine.Status < MachineStateType.IsSolving ? ValueState.Constant : ValueState.Defined;

            AssignEmptyDomain();
        }
    }
}
