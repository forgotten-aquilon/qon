using System;
using System.Collections.Generic;
using System.Text;
using qon.Layers.VariableLayers;
using qon.Variables;

namespace qon.QSL
{
    public static class QObjectSpatialExtensions
    {
        public static EuclideanLayer<TQ> OnEuclideanLayer<TQ>(this QObject<TQ> obj) where TQ : notnull
        {
            return EuclideanLayer<TQ>.On(obj);
        }
    }
}
