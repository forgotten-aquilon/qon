namespace qon.Layers
{
    public interface ILayerHolder<T, TSelf> 
    {
        LayersManager<T, TSelf> Layers { get; }
    }
}
