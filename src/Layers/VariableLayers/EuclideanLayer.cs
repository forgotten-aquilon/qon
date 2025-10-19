using qon.Helpers;
using qon.Machines;
using qon.Variables;

namespace qon.Layers.VariableLayers
{
    public class EuclideanLayer<T> : BaseLayer<T, EuclideanLayer<T>, QVariable<T>>, ILayer<T, QVariable<T>>
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public WFCMachine<T>? Machine { get; protected set; }

        public EuclideanLayer()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }

        public EuclideanLayer(int x, int y, int z, WFCMachine<T> machine)
        {
            X = x;
            Y = y;
            Z = z;
            Machine = machine;
        }

        ILayer<T, QVariable<T>> ICopy<ILayer<T, QVariable<T>>>.Copy()
        {
            return new EuclideanLayer<T>(X, Y, Z, Machine!);
        }
    }
}
