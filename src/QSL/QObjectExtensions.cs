using System;
using System.Collections.Generic;
using System.Text;
using qon.Layers.VariableLayers;
using qon.Variables;

namespace qon.QSL
{
    public static class QObjectExtensions
    {
        public static DomainLayer<TQ> OnDomainLayer<TQ>(this QObject<TQ> obj) where TQ : notnull
        {
            return DomainLayer<TQ>.On(obj);
        }

        public static EuclideanLayer<TQ> OnEuclideanLayer<TQ>(this QObject<TQ> obj) where TQ : notnull
        {
            return EuclideanLayer<TQ>.On(obj);
        }
    }
}
