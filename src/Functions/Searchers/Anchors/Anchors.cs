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
                IChain<QVariable<TQ>, QVariable<TQ>[]> filter = new VonNeumannFilter<TQ>() as IChain<QVariable<TQ>, QVariable<TQ>[]>;
                return filter.ApplyTo(a);
            });
        }
    }
}
