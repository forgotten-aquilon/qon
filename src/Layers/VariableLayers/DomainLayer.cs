using qon.Domains;
using qon.Exceptions;
using qon.Helpers;
using qon.Variables;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace qon.Layers.VariableLayers
{
    public class DomainLayer<T> : BaseLayer<T, DomainLayer<T>, QVariable<T>>, ILayer<T, QVariable<T>>
    {
        private IDomain<T> _domain;

        public IDomain<T> Domain
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

        public bool ContainsValue(T value)
        {
            return Domain.ContainsValue(value);
        }

        public int Size()
        {
            return Domain.Size();
        }

        public int RemoveValue(T value)
        {
            return Domain.Remove(value);
        }

        public int RemoveValues(IEnumerable<T> values)
        {
            ExceptionHelper.ThrowIfArgumentIsNull(values, nameof(values));
            return Domain.Remove(values);
        }

        public void AssignDomain(IDomain<T> domain)
        {
            Domain = domain;
        }

        public void AssignEmptyDomain()
        {
            Domain = EmptyDomain<T>.Instance;
        }

        public T GetRandomValue(Random random)
        {
            ExceptionHelper.ThrowIfArgumentIsNull(random, nameof(random));
            return Domain.GetRandomValue(random);
        }

        public Optional<T> SingleOrEmptyValue()
        {
            return Domain.SingleOrEmptyValue();
        }

        public void Collapse(T value, bool isConstant = false)
        {
            Holder.WithValue(value, isConstant ? ValueState.Constant : ValueState.Defined);
            AssignEmptyDomain();
        }

        public bool MatchesDomain(Func<IDomain<T>, bool> predicate)
        {
            ExceptionHelper.ThrowIfArgumentIsNull(predicate, nameof(predicate));
            return predicate(Domain);
        }

        public bool DomainSizeSatisfies(Func<int, bool> predicate)
        {
            ExceptionHelper.ThrowIfArgumentIsNull(predicate, nameof(predicate));
            return predicate(Size());
        }

        public TResult WithDomain<TResult>(Func<IDomain<T>, TResult> selector)
        {
            ExceptionHelper.ThrowIfArgumentIsNull(selector, nameof(selector));
            return selector(Domain);
        }

        public string DescribeDomain()
        {
            return Domain.ToString() ?? string.Empty;
        }

        public IDomain<T> GetDomain()
        {
            return Domain;
        }

        public bool TryGetDomain<TDomain>(out TDomain? domain) where TDomain : class, IDomain<T>
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
            : this(EmptyDomain<T>.Instance)
        {
        }

        public DomainLayer(IDomain<T> domain)  
        {
            Domain = domain;
        }

        ILayer<T, QVariable<T>> ICopy<ILayer<T, QVariable<T>>>.Copy()
        {
            return new DomainLayer<T>(Domain.Copy());
        }
    }
}
