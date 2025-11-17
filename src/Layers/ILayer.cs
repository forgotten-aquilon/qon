using qon.Helpers;

namespace qon.Layers
{
    public interface ILayer<TQ, THolder> : ICopy<ILayer<TQ, THolder>>
        where TQ : notnull
        where THolder : ILayerHolder<TQ, THolder>
    {
        public void UpdateManager(LayersManager<TQ, THolder> manager);
    }
}

