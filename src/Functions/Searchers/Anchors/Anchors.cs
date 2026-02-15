using qon.Functions.Filters;
using qon.Variables;

namespace qon.Functions.Searchers.Anchors
{
    public static class Anchors
    {
        public static Anchor<TQ> VNA<TQ>(QPredicate<TQ> predicate) where TQ : notnull
        {
            return new Anchor<TQ>(predicate, (f, a) =>
            {
                IChain<QVariable<TQ>, QVariable<TQ>[]> filter = VonNeumannFilter<TQ>.Filter;
                return filter.ApplyTo(a);
            });
        }
    }
}
