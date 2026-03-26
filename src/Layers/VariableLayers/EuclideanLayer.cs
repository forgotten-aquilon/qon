using System;
using qon.Helpers;
using qon.Machines;
using qon.Variables;
using System.Collections.Generic;
using qon.Layers.StateLayers;

namespace qon.Layers.VariableLayers
{
    public class EuclideanLayer<TQ> : BaseLayer<TQ, EuclideanLayer<TQ>, QObject<TQ>>, ILayer<TQ, QObject<TQ>> where TQ : notnull
    {
        public EuclideanStateLayer<TQ> StateLayer => EuclideanStateLayer<TQ>.With(Machine.State);

        public int X => StateLayer.Coordinates[Holder.Id].X;
        public int Y => StateLayer.Coordinates[Holder.Id].Y;
        public int Z => StateLayer.Coordinates[Holder.Id].Z;

        #region Overrides of BaseLayer<TQ,EuclideanLayer<TQ>,QObject<TQ>>

        public override ILayer<TQ, QObject<TQ>> Copy()
        {
            return new EuclideanLayer<TQ>()
            {
                NullableManager = NullableManager
            };
        }

        #endregion

        public override bool Equals(ILayer<TQ, QObject<TQ>> other)
        {
            if (other is EuclideanLayer<TQ> otherLayer)
            {
                return this.X == otherLayer.X && this.Y == otherLayer.Y && this.Z == otherLayer.Z;
            }

            return base.Equals(other);
        }
    }
}
