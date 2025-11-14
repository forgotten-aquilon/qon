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
        protected THolder? _holder;

        public THolder Holder => ExceptionHelper.ThrowIfFieldIsNull(_holder, nameof(_holder));

        public QMachine<T> Machine => ExceptionHelper.ThrowIfInternalValueIsNull(Holder?.Layers.Machine, nameof(Holder.Layers.Machine));

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
                layer._holder = holder;
            }

            return layer;
        }
    }
}
