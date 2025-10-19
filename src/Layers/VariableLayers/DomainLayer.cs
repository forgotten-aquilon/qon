using qon.Domains;
using qon.Exceptions;
using qon.Helpers;
using qon.Variables;

namespace qon.Layers.VariableLayers
{
    public class DomainLayer<T> : BaseLayer<T, DomainLayer<T>, QVariable<T>>, ILayer<T, QVariable<T>>
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

        public double Entropy => Domain.GetEntropy();

        public DomainLayer()
            : this(new DiscreteDomain<T>())
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
