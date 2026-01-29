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
    /// <summary>
    /// Manager of Layers, attached to an object
    /// </summary>
    /// <typeparam name="TQ">
    /// Key generic parameter
    /// </typeparam>
    /// <typeparam name="THolder">
    /// Type, which can hold this layer, e.g. <see cref="QVariable{TQ}"/> or <see cref="MachineState{TQ}"/>
    /// </typeparam>
    public class LayersManager<TQ, THolder> : KeyedCollection<Type, ILayer<TQ, THolder>>
        where TQ : notnull
        where THolder : ILayerHolder<TQ, THolder>
    {
        /// <summary>
        /// Layers sorted by its priority index
        /// </summary>
        private ILayer<TQ, THolder>[] _sortedLayers = Array.Empty<ILayer<TQ, THolder>>();

        /// <summary>
        /// Instance of the Holder object
        /// </summary>
        public THolder Holder { get; private set; }

        /// <summary>
        /// Instance of the Solution Machine
        /// </summary>
        public QMachine<TQ> Machine => Holder.Machine;

        /// <summary>
        /// Layers sorted by its priority index. Enumeration over this collection should be used when Layer's priority
        /// matters
        /// </summary>
        public IReadOnlyList<ILayer<TQ, THolder>> Layers
        {
            get
            {
                if (_sortedLayers.Length != Items.Count)
                {
                    _sortedLayers = Items
                        .OrderBy(layer => (layer as BaseLayer)?.PriorityIndex ?? int.MaxValue)
                        .ToArray();
                }

                return _sortedLayers;
            }
        }

        /// <summary>
        /// Create the new manager, attached to the Holder object
        /// </summary>
        /// <param name="holder"></param>
        public LayersManager(THolder holder)
        {
            Holder = holder;
        }


        /// <summary>Tries to get an item from the collection using the specified key.</summary>
        /// <param name="key">The key of the item to search in the collection.</param>
        /// <param name="item">When this method returns <see langword="true" />, the item from the collection that matches the provided key; when this method returns <see langword="false" />, the <see langword="default" /> value for the type of the collection.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> is <see langword="null" />.</exception>
        /// <returns>
        /// <see langword="true" /> if an item for the specified key was found in the collection; otherwise, <see langword="false" />.</returns>
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
                return l;
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
