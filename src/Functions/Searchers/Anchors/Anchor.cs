using System;
using qon.Functions.Filters;
using qon.Variables;

namespace qon.Functions.Searchers.Anchors
{
    public class Anchor<TQ> : IAnchor<TQ> where TQ : notnull
    {
        public QPredicate<TQ> Predicate { get; set; }
        public Func<QObject<TQ>[], QObject<TQ>, QObject<TQ>[]> Func { get; set; }

        public Anchor(QPredicate<TQ> predicate, Func<QObject<TQ>[], QObject<TQ>, QObject<TQ>[]> func)
        {
            Predicate = predicate;
            Func = func;
        }

        public QPredicate<TQ> GetPredicate()
        {
            return Predicate;
        }

        public QObject<TQ>[] GetAnchoredVariables(QObject<TQ>[] field, QObject<TQ> anchor)
        {
            return Func(field, anchor);
        }
    }
}
