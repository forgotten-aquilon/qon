using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using qon.Layers.StateLayers;
using qon.Machines;
using qon.Variables;

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

        public static QVariable<TQ> Collapse<TQ>(this QVariable<TQ> variable, TQ value, bool isConstant = false) where TQ : notnull
        {
            ConstraintLayer<TQ>.Collapse(variable, value, isConstant);

            return variable;
        }
    }
}
