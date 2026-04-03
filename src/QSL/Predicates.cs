using qon.Functions.Constraints;
using qon.Functions.Filters;
using qon.Layers;
using qon.Variables;
using System;
using System.Collections.Generic;
using System.Text;

namespace qon.QSL
{
    public static class Predicates
    {
        public static Func<QObject<TQ>, QPredicate<TQ>> OnLayer<TQ, TLayer>(Func<TLayer, QPredicate<TQ>> predicate) where TQ : notnull
            where TLayer : ILayer<TQ, QObject<TQ>>
        {
            return RelativeConstraint<TQ>.WithLayer(predicate);
        }
    }
}
