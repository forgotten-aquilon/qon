using qon.Helpers;

namespace qon.Layers
{
    public interface ILayer<T, THolder> : ICopy<ILayer<T, THolder>> where THolder : ILayerHolder<T, THolder>
    {
        public void UpdateHolder(THolder holder);
    }
}

