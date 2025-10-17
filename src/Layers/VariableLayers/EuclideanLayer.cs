using qon.Helpers;
using qon.Variables;

namespace qon.Layers.VariableLayers
{
    public class EuclideanLayer<T> : ILayer<T, QVariable<T>>
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

        public static EuclideanLayer<T>? For(QVariable<T> variable)
        {
            if (variable.Layers.TryGetLayer<EuclideanLayer<T>>(out var layer))
            {
                return layer;
            }

            return null;
        }

        ILayer<T, QVariable<T>> ICopy<ILayer<T, QVariable<T>>>.Copy()
        {
            return new EuclideanLayer<T>(X, Y, Z, Machine);
        }
    }
}
