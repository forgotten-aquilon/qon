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

    public abstract class BaseLayer<T, TSelf, THolder> : BaseLayer
        where TSelf : BaseLayer<T, TSelf, THolder>, ILayer<T, THolder>, new()
        where THolder : ILayerHolder<T, THolder>
    {
        public THolder? Holder {get; set; }

        public QMachine<T> Machine
        {
            get
            {
                //TODO: Add new generic method to helper later
                ExceptionHelper.ThrowIfInternalValueIsNull(Holder?.Layers.Machine, nameof(Holder.Layers.Machine));
                return Holder.Layers.Machine;
            }
        }

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
                layer.Holder = holder;
            }

            return layer;
        }
    }
}
