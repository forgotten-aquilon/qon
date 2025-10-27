using System;
using System.Collections.Generic;
using System.Linq;
using qon.Functions.Filters;
using qon.Variables;

namespace qon.Functions.Anchors
{
    public interface IAnchor<T>
    {
        public QPredicate<T> GetPredicate();

        public QVariable<T>[] GetAnchoredVariables(QVariable<T>[] field, QVariable<T> anchor);
    }
}
