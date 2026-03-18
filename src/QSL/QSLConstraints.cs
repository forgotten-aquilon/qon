using System;
using System.Collections.Generic;
using System.Text;
using qon.Layers.StateLayers;
using qon.Machines;

namespace qon
{
    public static partial class QSL
    {
        public static QSLConstraintsBuilder<TQ> CreateConstraint<TQ>() where TQ : notnull
        {
            return new QSLConstraintsBuilder<TQ>();
        }

        public static QMachine<TQ> WithConstraint<TQ>(this QMachine<TQ> machine, ConstraintLayerParameter<TQ> parameter) where TQ : notnull
        {
            ConstraintLayer<TQ>.GetOrCreate(machine.State).Constraints = parameter;

            return machine;
        }
    }
}
