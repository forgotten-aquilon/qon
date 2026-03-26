using qon.Exceptions;
using qon.Helpers;
using qon.Layers.StateLayers;
using qon.Layers.VariableLayers;
using qon.Machines;
using qon.Variables;
using qon.Variables.Domains;
using System;
using System.Collections.Generic;
using System.Text;

namespace qon
{
    public static partial class QSL
    {
        public static QMachine<TQ> Machine<TQ>(QMachineParameter<TQ>? parameter = null) where TQ : notnull
        {
            return new QMachine<TQ>(parameter.NewOrExisting());
        }

        public static QMachine<TQ> GenerateField<TQ>(this QMachine<TQ> machine, IDomain<TQ> domain, int count) where TQ : notnull
        {
            var field = new List<QObject<TQ>>();
            for (int i = 0; i < count; i++)
            {
                var variable = QObject<TQ>.Empty();
                DomainLayer<TQ>.GetOrCreate(variable).AssignDomain(domain);
                field.Add(variable);
            }
            machine.InitializeField(field);

            return machine;
        }

        public static QMachine<TQ> GenerateField<TQ>(this QMachine<TQ> machine, IDomain<TQ> domain, IEnumerable<string> names) where TQ : notnull
        {
            var field = new List<QObject<TQ>>();
            foreach (var name in names)
            {
                var variable = QObject<TQ>.Empty(name);
                DomainLayer<TQ>.GetOrCreate(variable).AssignDomain(domain);
                field.Add(variable);
            }
            machine.InitializeField(field);

            return machine;
        }

        public static QMachine<TQ> GenerateField<TQ>(this QMachine<TQ> machine, IDomain<TQ>? domain, (int x, int y, int z) dimensions, Optional<TQ> defaultValue = new Optional<TQ>()) where TQ : notnull
        {
            List<QObject<TQ>> variables = new();

            if (dimensions.x < 1 || dimensions.y < 1 || dimensions.z < 1)
            {
                throw new InternalLogicException("Dimension can't be a non-positive number");
            }

            var layer = EuclideanStateLayer<TQ>.GetOrCreate(machine.State);
            layer.UpdateSize(dimensions.x, dimensions.y, dimensions.z);

            for (int z = 0; z < dimensions.z; z++)
            {
                for (int y = 0; y < dimensions.y; y++)
                {
                    for (int x = 0; x < dimensions.x; x++)
                    {
                        string name = $"{x}x{y}x{z}";

                        QObject<TQ> newObject = defaultValue.HasValue
                            ? QObject<TQ>.New(name, defaultValue.Value)
                            : QObject<TQ>.Empty(name);

                        if (domain is { } d)
                        {
                            DomainLayer<TQ>.GetOrCreate(newObject).AssignDomain(d);
                        }

                        EuclideanLayer<TQ>.GetOrCreate(newObject);

                        layer.FieldGrid[x, y, z] = newObject.Id;
                        layer.Coordinates[newObject.Id] = (x, y, z);
                        variables.Add(newObject);
                    }
                }
            }

            machine.InitializeField(variables);

            return machine;
        }
    }
}
