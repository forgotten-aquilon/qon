using System;
using qon.Functions.Filters;
using qon.Variables;

namespace qon.Functions.Searchers.Anchors
{
    public class Anchor<TQ> : IAnchor<TQ> where TQ : notnull
    {
        public QPredicate<TQ> Predicate { get; set; }
        public Func<QVariable<TQ>[], QVariable<TQ>, QVariable<TQ>[]> Func { get; set; }

        public Anchor(QPredicate<TQ> predicate, Func<QVariable<TQ>[], QVariable<TQ>, QVariable<TQ>[]> func)
        {
            Predicate = predicate;
            Func = func;
        }

        public QPredicate<TQ> GetPredicate()
        {
            return Predicate;
        }

        public QVariable<TQ>[] GetAnchoredVariables(QVariable<TQ>[] field, QVariable<TQ> anchor)
        {
            return Func(field, anchor);
        }
    }
}
