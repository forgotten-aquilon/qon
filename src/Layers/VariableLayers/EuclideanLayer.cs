using qon.Helpers;
using qon.Machines;
using qon.Variables;

namespace qon.Layers.VariableLayers
{
    public class EuclideanLayer<TQ> : BaseLayer<TQ, EuclideanLayer<TQ>, QVariable<TQ>>, ILayer<TQ, QVariable<TQ>> where TQ : notnull
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public EuclideanLayer()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }

        public EuclideanLayer(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public void Update(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        ILayer<TQ, QVariable<TQ>> ICopy<ILayer<TQ, QVariable<TQ>>>.Copy()
        {
            return new EuclideanLayer<TQ>(X, Y, Z);
        }

        #region Overrides of BaseLayer<TQ,EuclideanLayer<TQ>,QVariable<TQ>>

        public override ILayer<TQ, QVariable<TQ>> Copy()
        {
            return new EuclideanLayer<TQ>(X, Y, Z);
        }

        #endregion
    }
}
