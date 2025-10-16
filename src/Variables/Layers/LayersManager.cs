using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Exceptions;

namespace qon.Variables.Layers
{
    public class LayersManager<T> : KeyedCollection<Type, ILayer<T>>
    {
        public bool TryGetLayer<TLayer>([NotNullWhen(true)]out TLayer? layer)  
        {
            if (TryGetValue(typeof(TLayer), out var l))
            {
                layer = (TLayer)l;
                return true;
            }

            layer = default;
            return false;
        }

        public ILayer<T>? GetLayerOrNull<TLayer>() where TLayer : ILayer<T>
        {
            TryGetValue(typeof(TLayer), out ILayer<T>? result);

            return result;
        }

        public ILayer<T> GetLayer<TLayer>() where TLayer : ILayer<T>
        {
            if (TryGetLayer<TLayer>(out var l))
            {
                return l!;
            }

            //TODO
            throw new InternalLogicException("");
        }

        protected override Type GetKeyForItem(ILayer<T> item)
        {
            return item.GetType();
        }

        public LayersManager<T> Copy()
        {
            var result = new LayersManager<T>();

            foreach (var item in this.Items)
            {
                result.Add(item.Copy());
            }

            return result;
        }

        public TLayer? With<TLayer>() where TLayer : ILayer<T>
        {
            if (TryGetLayer<TLayer>(out var layer))
            {
                return layer;
            }

            return default;
        }
    }
}
