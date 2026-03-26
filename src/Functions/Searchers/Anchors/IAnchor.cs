using qon.Functions.Filters;
using qon.Variables;

namespace qon.Functions.Searchers.Anchors
{
    public interface IAnchor<TQ> where TQ : notnull
    {
        public QPredicate<TQ> GetPredicate();

        public QObject<TQ>[] GetAnchoredVariables(QObject<TQ>[] field, QObject<TQ> anchor);
    }
}
