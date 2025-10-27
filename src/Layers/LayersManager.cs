using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using qon.Exceptions;
using qon.Machines;

namespace qon.Layers
{
    public class LayersManager<T, THolder> : KeyedCollection<Type, ILayer<T, THolder>>
    {
        public QMachine<T>? Machine { get; set; }

        public LayersManager(QMachine<T>? machine = null)
        {
            Machine = machine;
        }

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

        public IEnumerable<ILayer<T, THolder>> SortedByPriority()
        {
            return Items
                .OrderBy(layer => (layer as BaseLayer)?.PriorityIndex ?? int.MaxValue)
                .ToArray();
        }
    }
}
