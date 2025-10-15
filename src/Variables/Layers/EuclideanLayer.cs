using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qon.Variables.Layers
{
    public class EuclideanLayer<T> : ILayer<T>
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public WFCMachine<T> Machine { get; protected set; }

        public EuclideanLayer(int x, int y, int z, WFCMachine<T> machine)
        {
            X = x;
            Y = y;
            Z = z;
            Machine = machine;
        }

        public ILayer<T> Copy()
        {
            return new EuclideanLayer<T>(X, Y, Z, Machine);
        }
    }
}
