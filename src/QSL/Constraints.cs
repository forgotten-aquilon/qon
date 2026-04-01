using qon.Functions;
using qon.Layers.StateLayers;
using qon.Machines;
using qon.Variables;

namespace qon.QSL
{
    public static class Constraints
    {
        public static ConstraintsBuilder<TQ> CreateConstraint<TQ>() where TQ : notnull
        {
            return new ConstraintsBuilder<TQ>();
        }

        public static QMachine<TQ> WithConstraintLayer<TQ>(this QMachine<TQ> machine, ConstraintLayerParameter<TQ> parameter) where TQ : notnull
        {
            ConstraintLayer<TQ>.GetOrCreate(machine.State).Constraints = parameter;

            return machine;
        }

        public static QMachine<TQ> AddConstraint<TQ>(this QMachine<TQ> machine, IPreparation<TQ> constraint) where TQ : notnull
        {
            ConstraintLayer<TQ>.GetOrCreate(machine.State).Constraints.GeneralConstraints.Add(constraint);

            return machine;
        }

        public static QMachine<TQ> AddValidation<TQ>(this QMachine<TQ> machine, IPreparation<TQ> validation) where TQ : notnull
        {
            ConstraintLayer<TQ>.GetOrCreate(machine.State).Constraints.ValidationConstraints.Add(validation);

            return machine;
        }

        public static QObject<TQ> Collapse<TQ>(this QObject<TQ> @object, TQ value, bool isConstant = false) where TQ : notnull
        {
            ConstraintLayer<TQ>.Collapse(@object, value, isConstant);

            return @object;
        }
    }
}
