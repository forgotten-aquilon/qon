using System;
using System.Collections.Generic;
using System.Text;
using qon.Layers.VariableLayers;
using qon.Variables;

namespace qon.QSL
{
    public static class QObjectSpatialExtensions
    {
        public static CartesianLayer<TQ> Cartesian<TQ>(this QObject<TQ> obj) where TQ : notnull
        {
            return CartesianLayer<TQ>.On(obj);
        }
    }
}
