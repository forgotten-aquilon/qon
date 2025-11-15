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

    public abstract class BaseLayer<T, TSelf, THolder> : BaseLayer, ILayer<T, THolder>
        where TSelf : BaseLayer<T, TSelf, THolder>, ILayer<T, THolder>, new()
        where THolder : ILayerHolder<T, THolder>
    {
        protected LayersManager<T, THolder>? _manager;

        public LayersManager<T, THolder> Manager => ExceptionHelper.ThrowIfFieldIsNull(_manager, nameof(_manager));

        public THolder Holder => ExceptionHelper.ThrowIfFieldIsNull(Manager.Holder, nameof(Manager.Holder));

        public QMachine<T> Machine => ExceptionHelper.ThrowIfFieldIsNull(Holder.Machine, nameof(Holder.Machine));

        public static TSelf With(THolder holder)
        {
            holder.Layers.TryGetLayer<TSelf>(out var layer);

            ExceptionHelper.ThrowIfInternalValueIsNull(layer, nameof(layer));

            return layer;
        }

        public static TSelf? From(THolder holder)
        {
            holder.Layers.TryGetLayer<TSelf>(out TSelf? layer);
            return layer;
        }

        public static TSelf GetOrCreate(THolder holder)
        {
            if (!holder.Layers.TryGetLayer<TSelf>(out var layer))
            {
                layer = new TSelf();
                holder.Layers.Add(layer);
                layer._manager = holder.Layers;
            }

            return layer;
        }

        #region Implementation of ICopy<ILayer<T,THolder>>

        public abstract ILayer<T, THolder> Copy();

        #endregion

        #region Implementation of ILayer<T,THolder>

        public void UpdateHolder(THolder holder)
        {
            _manager = holder.Layers;
        }

        #endregion
    }
}
