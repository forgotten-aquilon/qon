using System;
using qon.Helpers;
using qon.Machines;
using qon.Variables;
using System.Collections.Generic;
using qon.Layers.StateLayers;

namespace qon.Layers.VariableLayers
{
    public class CartesianLayer<TQ> : BaseLayer<TQ, CartesianLayer<TQ>, QObject<TQ>>, ILayer<TQ, QObject<TQ>> where TQ : notnull
    {
        public CartesianStateLayer<TQ> StateLayer => CartesianStateLayer<TQ>.On(Machine.State);

        public int X => StateLayer.Coordinates[Holder.Id].X;
        public int Y => StateLayer.Coordinates[Holder.Id].Y;
        public int Z => StateLayer.Coordinates[Holder.Id].Z;

        #region Overrides of BaseLayer<TQ,CartesianLayer<TQ>,QObject<TQ>>

        public override ILayer<TQ, QObject<TQ>> Copy()
        {
            return new CartesianLayer<TQ>()
            {
                NullableManager = NullableManager
            };
        }

        #endregion

        public override bool Equals(ILayer<TQ, QObject<TQ>> other)
        {
            if (base.Equals(other))
            {
                return true;
            }

            if (other is CartesianLayer<TQ> otherLayer)
            {
                return this.X == otherLayer.X && this.Y == otherLayer.Y && this.Z == otherLayer.Z;
            }

            return false;
        }
    }
}
