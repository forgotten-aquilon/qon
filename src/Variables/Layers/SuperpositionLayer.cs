using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using qon.Domains;
using qon.Exceptions;
using qon.Helpers;

namespace qon.Variables.Layers
{
    public class SuperpositionLayer<T> : ILayer<T>
    {
        private IDomain<T> _domain = EmptyDomain<T>.Instance;

        public IDomain<T> Domain
        {
            get => _domain;
            set
            {
                ExceptionHelper.ThrowIfArgumentIsNull(value, nameof(value));
                _domain = value.Copy();
            }
        }

        public SuperpositionState State { get; set; }

        public double Entropy => Domain.GetEntropy();

        public SuperpositionLayer()
            : this(new DiscreteDomain<T>())
        {
        }

        public SuperpositionLayer(IDomain<T> domain, SuperpositionState state = SuperpositionState.Uncertain)
        {
            Domain = domain;
            State = state;
        }

        public int SetDomain(IDomain<T> domain)
        {
            if (State != SuperpositionState.Uncertain)
            {
                Domain = EmptyDomain<T>.Instance;
                return 0;
            }

            Domain = domain;
            return Domain.Size();
        }

        public void Collapse(bool isConstant)
        {
            State = isConstant ? SuperpositionState.Constant : SuperpositionState.Defined;
            Domain = EmptyDomain<T>.Instance;
        }

        public Optional<T> AutoCollapse()
        {
            return Domain.SingleOrEmptyValue();
        }

        public ILayer<T> Copy()
        {
            return new SuperpositionLayer<T>(Domain.Copy(), State);
        }

        //Use for initialization of a layer
        public static SuperpositionLayer<T> For(QVariable<T> variable)
        {
            if (!variable.Layers.TryGetLayer<SuperpositionLayer<T>>(out var layer))
            {
                layer = new SuperpositionLayer<T>();
                variable.Layers.Add(layer);
            }

            return layer;
        }

        //Use for retrieving of a layer, when you are absolutely sure it exists
        public static SuperpositionLayer<T> With(QVariable<T> variable)
        {
            if (variable.Layers.TryGetLayer<SuperpositionLayer<T>>(out var layer))
            {
                return layer;
            }

            //TODO
            throw new InternalLogicException("");
        }

        //Try to get a layer from variable
        public static SuperpositionLayer<T>? From(QVariable<T> variable)
        {
            variable.Layers.TryGetLayer<SuperpositionLayer<T>>(out SuperpositionLayer<T>? result);

            return result;
        }

        public static void Collapse(QVariable<T> variable, T value, bool isConstant = false)
        {
            variable.UpdateValue(value);
            For(variable).Collapse(isConstant);
        }

        public static Optional<T> AutoCollapse(QVariable<T> variable)
        {
            var layer = For(variable);
            var value = layer.AutoCollapse();

            if (value.HasValue)
            {
                Collapse(variable, value.Value);
            }

            return value;
        }
    }
}
