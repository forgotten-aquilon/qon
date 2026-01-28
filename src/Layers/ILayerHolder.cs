using qon.Machines;

namespace qon.Layers
{
    public interface ILayerHolder<TQ, TSelf> where TSelf : ILayerHolder<TQ, TSelf> where TQ : notnull
    {
        LayersManager<TQ, TSelf> LayerManager { get; }
        QMachine<TQ> Machine { get; }

    }
}