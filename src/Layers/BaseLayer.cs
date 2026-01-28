using qon.Exceptions;
using qon.Layers.VariableLayers;
using qon.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Machines;

namespace qon.Layers
{
    public abstract class BaseLayer
    {
        public int PriorityIndex { get; set; } = 0;
    }

    public abstract class BaseLayer<TQ, TSelf, THolder> : BaseLayer, ILayer<TQ, THolder>
        where TQ : notnull
        where TSelf : BaseLayer<TQ, TSelf, THolder>, ILayer<TQ, THolder>, new()
        where THolder : ILayerHolder<TQ, THolder>
    {
        protected LayersManager<TQ, THolder>? _manager;

        public LayersManager<TQ, THolder> Manager => ExceptionHelper.ThrowIfFieldIsNull(_manager, nameof(_manager));

        public THolder Holder => ExceptionHelper.ThrowIfFieldIsNull(Manager.Holder, nameof(Manager.Holder));

        public QMachine<TQ> Machine => ExceptionHelper.ThrowIfFieldIsNull(Holder.Machine, nameof(Holder.Machine));

        public static TSelf With(THolder holder)
        {
            holder.LayerManager.TryGetLayer<TSelf>(out var layer);

            ExceptionHelper.ThrowIfInternalValueIsNull(layer, nameof(layer));

            return layer;
        }

        public static TSelf? From(THolder holder)
        {
            holder.LayerManager.TryGetLayer<TSelf>(out TSelf? layer);
            return layer;
        }

        public static TSelf GetOrCreate(THolder holder)
        {
            if (!holder.LayerManager.TryGetLayer<TSelf>(out var layer))
            {
                layer = new TSelf();
                holder.LayerManager.Add(layer);
                layer._manager = holder.LayerManager;
            }

            return layer;
        }

        #region Implementation of ICopy<ILayer<T,THolder>>

        public abstract ILayer<TQ, THolder> Copy();

        #endregion

        #region Implementation of ILayer<T,THolder>

        public void UpdateManager(LayersManager<TQ, THolder> manager)
        {
            _manager = manager;
        }

        #endregion
    }
}
