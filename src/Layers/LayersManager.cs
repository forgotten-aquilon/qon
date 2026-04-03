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
    /// Type, which can hold this layer, e.g. <see cref="QObject{TQ}"/> or <see cref="MachineState{TQ}"/>
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
        /// <param name="holder">Holder instance that owns this manager.</param>
        public LayersManager(THolder holder)
        {
            Holder = holder;
        }


        /// <summary>
        /// Try to resolve a layer by type.
        /// </summary>
        /// <typeparam name="TLayer">Layer type to resolve.</typeparam>
        /// <param name="layer">
        /// When this method returns <see langword="true"/>, the resolved layer; otherwise <see langword="default"/>.
        /// </param>
        /// <returns><see langword="true"/> if the layer type was found; otherwise <see langword="false"/>.</returns>
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

        /// <summary>
        /// Get a layer by type or <see langword="null"/> if it is not registered.
        /// </summary>
        /// <typeparam name="TLayer">Layer type to resolve.</typeparam>
        /// <returns>The registered layer instance or <see langword="null"/>.</returns>
        public ILayer<TQ, THolder>? GetLayerOrNull<TLayer>() where TLayer : ILayer<TQ, THolder>
        {
            TryGetValue(typeof(TLayer), out ILayer<TQ, THolder>? result);

            return result;
        }

        /// <summary>
        /// Get a layer by type or throw if it is not registered.
        /// </summary>
        /// <typeparam name="TLayer">Layer type to resolve.</typeparam>
        /// <returns>The registered layer instance.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the layer type is not registered.</exception>
        public ILayer<TQ, THolder> GetLayer<TLayer>() where TLayer : ILayer<TQ, THolder>
        {
            if (TryGetLayer<TLayer>(out var l))
            {
                return l;
            }

            throw new KeyNotFoundException($"Layer of Type {typeof(TLayer)} is not registered in LayersManager of {typeof(THolder)}:{Holder}");
        }

        /// <summary>
        /// Get the collection key for a layer.
        /// </summary>
        /// <param name="item">Layer instance.</param>
        /// <returns>Concrete layer type used as the key.</returns>
        protected override Type GetKeyForItem(ILayer<TQ, THolder> item)
        {
            return item.GetType();
        }

        /// <summary>
        /// Return a layer if present, otherwise <see langword="default"/>.
        /// </summary>
        /// <typeparam name="TLayer">Layer type to resolve.</typeparam>
        /// <returns>The resolved layer or <see langword="default"/>.</returns>
        public TLayer? With<TLayer>() where TLayer : ILayer<TQ, THolder>
        {
            if (TryGetLayer<TLayer>(out var layer))
            {
                return layer;
            }

            return default;
        }

        /// <summary>
        /// Copy layers from another manager into this manager and rebind them to this instance.
        /// </summary>
        /// <param name="layers">Source manager to copy from.</param>
        public void Add(LayersManager<TQ, THolder> layers)
        {
            foreach (var layer in layers)
            {
                var newLayer = layer.Copy();
                newLayer.AttachManager(this);
                this.Add(newLayer);
            }
        }
    }
}
