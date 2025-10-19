using qon.Exceptions;
using qon.Layers.VariableLayers;
using qon.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qon.Layers
{
    public abstract class BaseLayer<T, TSelf, THolder> where TSelf : BaseLayer<T, TSelf, THolder>, ILayer<T, THolder>, new()
        where THolder : ILayerHolder<T, THolder>
    {
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

        public static TSelf TryCreate(THolder holder)
        {
            if (!holder.Layers.TryGetLayer<TSelf>(out var layer))
            {
                layer = new TSelf();
                holder.Layers.Add(layer);
            }

            return layer;
        }
    }
}
