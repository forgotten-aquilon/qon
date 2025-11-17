using System;
using System.Collections.Generic;
using System.Linq;
using qon.Functions.Filters;
using qon.Variables;

namespace qon.Functions.Anchors
{
    public interface IAnchor<TQ> where TQ : notnull
    {
        public QPredicate<TQ> GetPredicate();

        public QVariable<TQ>[] GetAnchoredVariables(QVariable<TQ>[] field, QVariable<TQ> anchor);
    }
}
