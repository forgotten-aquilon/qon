using qon.Exceptions;
using qon.Helpers;
using qon.Variables;
using qon.Variables.Domains;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace qon.Layers.VariableLayers
{
    public class DomainLayer<TQ> : BaseLayer<TQ, DomainLayer<TQ>, QVariable<TQ>>, ILayer<TQ, QVariable<TQ>> where TQ : notnull
    {
        private IDomain<TQ> _domain;

        public IDomain<TQ> Domain
        {
            get => _domain;

            //Can't wait to use 'field' keyword when Unity finally switches to CoreCLR
            [MemberNotNull(nameof(_domain))]
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

        public void Collapse(TQ value, bool isConstant = false)
        {
            Holder.WithValue(value, isConstant ? ValueState.Constant : ValueState.Defined);
            AssignEmptyDomain();
        }

        public bool MatchesDomain(Func<IDomain<TQ>, bool> predicate)
        {
            ExceptionHelper.ThrowIfArgumentIsNull(predicate, nameof(predicate));
            return predicate(Domain);
        }

        public bool DomainSizeSatisfies(Func<int, bool> predicate)
        {
            ExceptionHelper.ThrowIfArgumentIsNull(predicate, nameof(predicate));
            return predicate(Size());
        }

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

        public bool TryGetDomain<TDomain>(out TDomain? domain) where TDomain : class, IDomain<TQ>
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
            Domain = domain;
        }

        #region Overrides of BaseLayer<TQ,DomainLayer<TQ>,QVariable<TQ>>

        public override ILayer<TQ, QVariable<TQ>> Copy()
        {
            return new DomainLayer<TQ>(Domain.Copy());
        }

        #endregion
    }
}