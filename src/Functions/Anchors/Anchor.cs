using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Functions.Filters;
using qon.Variables;

namespace qon.Functions.Anchors
{
    public class Anchor<T> : IAnchor<T>
    {
        public QPredicate<T> Predicate { get; set; }
        public Func<QVariable<T>[], QVariable<T>, QVariable<T>[]> Func { get; set; }

        public Anchor(QPredicate<T> predicate, Func<QVariable<T>[], QVariable<T>, QVariable<T>[]> func)
        {
            Predicate = predicate;
            Func = func;
        }

        public QPredicate<T> GetPredicate()
        {
            return Predicate;
        }

        public QVariable<T>[] GetAnchoredVariables(QVariable<T>[] field, QVariable<T> anchor)
        {
            return Func(field, anchor);
        }
    }
}
