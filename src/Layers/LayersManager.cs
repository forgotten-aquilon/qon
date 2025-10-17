using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using qon.Exceptions;

namespace qon.Layers
{
    public class LayersManager<T, THolder> : KeyedCollection<Type, ILayer<T, THolder>>
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

        public ILayer<T, THolder>? GetLayerOrNull<TLayer>() where TLayer : ILayer<T, THolder>
        {
            TryGetValue(typeof(TLayer), out ILayer<T, THolder>? result);

            return result;
        }

        public ILayer<T, THolder> GetLayer<TLayer>() where TLayer : ILayer<T, THolder>
        {
            if (TryGetLayer<TLayer>(out var l))
            {
                return l!;
            }

            //TODO
            throw new InternalLogicException("");
        }

        protected override Type GetKeyForItem(ILayer<T, THolder> item)
        {
            return item.GetType();
        }

        public LayersManager<T, THolder> Copy()
        {
            var result = new LayersManager<T, THolder>();

            foreach (var item in this.Items)
            {
                result.Add(item.Copy());
            }

            return result;
        }

        public TLayer? With<TLayer>() where TLayer : ILayer<T, THolder>
        {
            if (TryGetLayer<TLayer>(out var layer))
            {
                return layer;
            }

            return default;
        }
    }
}
