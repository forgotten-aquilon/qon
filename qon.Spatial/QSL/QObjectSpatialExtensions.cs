using qon.Layers.StateLayers;
using qon.Layers.VariableLayers;
using qon.Variables;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace qon.QSL
{
    public static class QObjectSpatialExtensions
    {
        public static CartesianLayer<TQ> Cartesian<TQ>(this QObject<TQ> obj) where TQ : notnull
        {
            return CartesianLayer<TQ>.On(obj);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static QObject<TQ>? GetNeighbor<TQ>(this QObject<TQ> anchor, int dx, int dy, int dz) where TQ : notnull
        {
            var layer = anchor.Cartesian();
            var stateLayer = CartesianStateLayer<TQ>.On(anchor.Machine.State);

            return stateLayer[(layer.X + dx, layer.Y + dy, layer.Z + dz)];
        }
    }
}
