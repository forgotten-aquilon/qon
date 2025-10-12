using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qon.Variables.Layers
{
    public class LayersManager<T> : KeyedCollection<Type, ILayer<T>>
    {
        public bool TryGetLayer<TLayer>(out TLayer? layer) where TLayer : ILayer<T>
        {
            if (TryGetValue(typeof(TLayer), out var l))
            {
                layer = (TLayer)l;
                return true;
            }

            layer = default;
            return false;
        }

        protected override Type GetKeyForItem(ILayer<T> item)
        {
            return item.GetType();
        }
    }
}
