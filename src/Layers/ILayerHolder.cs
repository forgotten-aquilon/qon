using qon.Machines;

namespace qon.Layers
{
    public interface ILayerHolder<T, TSelf> where TSelf : ILayerHolder<T, TSelf>
    {
        LayersManager<T, TSelf> Layers { get; }
        QMachine<T> Machine { get; }

    }
}