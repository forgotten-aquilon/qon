using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qon.Layers.StateLayers
{
    public class EuclideanStateLayer<T> : ILayer<T, MachineState<T>>
    {
        protected string[,,] FieldGrid { get; set; } = new string[0, 0, 0];
        public ILayer<T, MachineState<T>> Copy()
        {
            throw new NotImplementedException();
        }
    }
}
