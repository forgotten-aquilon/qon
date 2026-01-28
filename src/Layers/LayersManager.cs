using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using qon.Exceptions;
using qon.Machines;
using qon.Variables;

namespace qon.Layers
{
    public class LayersManager<TQ, THolder> : KeyedCollection<Type, ILayer<TQ, THolder>>
        where TQ : notnull
        where THolder : ILayerHolder<TQ, THolder>
    {
        private ILayer<TQ, THolder>[] _sortedLayers = Array.Empty<ILayer<TQ, THolder>>();

        public THolder Holder { get; private set; }
        public QMachine<TQ> Machine => Holder.Machine;


        //Basically initialized only once and never updated
        public IReadOnlyList<ILayer<TQ, THolder>> Layers
        {
            get
            {
                if (_sortedLayers.Length == 0)
                {
                    _sortedLayers = Items
                        .OrderBy(layer => (layer as BaseLayer)?.PriorityIndex ?? int.MaxValue)
                        .ToArray();
                }

                return _sortedLayers;
            }
        }

        public LayersManager(THolder holder)
        {
            Holder = holder;
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

        public ILayer<TQ, THolder>? GetLayerOrNull<TLayer>() where TLayer : ILayer<TQ, THolder>
        {
            TryGetValue(typeof(TLayer), out ILayer<TQ, THolder>? result);

            return result;
        }

        public ILayer<TQ, THolder> GetLayer<TLayer>() where TLayer : ILayer<TQ, THolder>
        {
            
            if (TryGetLayer<TLayer>(out var l))
            {
                return l!;
            }

            throw new KeyNotFoundException($"Layer of Type {typeof(TLayer)} is not registered in LayersManager of {typeof(THolder)}:{Holder}");
        }

        protected override Type GetKeyForItem(ILayer<TQ, THolder> item)
        {
            return item.GetType();
        }

        public TLayer? With<TLayer>() where TLayer : ILayer<TQ, THolder>
        {
            if (TryGetLayer<TLayer>(out var layer))
            {
                return layer;
            }

            return default;
        }

        public void Add(LayersManager<TQ, THolder> layers)
        {
            foreach (var layer in layers)
            {
                var newLayer = layer.Copy();
                newLayer.UpdateManager(this);
                this.Add(newLayer);
            }
        }
    }
}
