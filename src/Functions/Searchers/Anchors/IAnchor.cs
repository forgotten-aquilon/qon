using qon.Functions.Filters;
using qon.Variables;

namespace qon.Functions.Searchers.Anchors
{
    public interface IAnchor<TQ> where TQ : notnull
    {
        public QPredicate<TQ> GetPredicate();

        public QVariable<TQ>[] GetAnchoredVariables(QVariable<TQ>[] field, QVariable<TQ> anchor);
    }
}
