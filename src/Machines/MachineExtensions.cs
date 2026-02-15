using qon.Exceptions;
using qon.Helpers;
using qon.Layers.StateLayers;
using qon.Layers.VariableLayers;
using qon.Variables;
using qon.Variables.Domains;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qon.Machines
{
    public static class MachineExtensions
    {
        public static void GenerateField<TQ>(this QMachine<TQ> machine, IDomain<TQ> domain, int count) where TQ : notnull
        {
            var field = new List<QVariable<TQ>>();
            for (int i = 0; i < count; i++)
            {
                var variable = QVariable<TQ>.Empty();
                DomainLayer<TQ>.GetOrCreate(variable).AssignDomain(domain);
                field.Add(variable);
            }
            machine.InitializeField(field);
        }

        public static void GenerateField<TQ>(this QMachine<TQ> machine, IDomain<TQ> domain, IEnumerable<string> names) where TQ : notnull
        {
            var field = new List<QVariable<TQ>>();
            foreach (var name in names)
            {
                var variable = QVariable<TQ>.Empty(name);
                DomainLayer<TQ>.GetOrCreate(variable).AssignDomain(domain);
                field.Add(variable);
            }
            machine.InitializeField(field);
        }


        public static void GenerateField<TQ>(this QMachine<TQ> machine, IDomain<TQ> domain, (int x, int y, int z) dimensions, Optional<TQ> defaultValue = new Optional<TQ>()) where TQ : notnull
        {
            List<QVariable<TQ>> variables = new();

            if (dimensions.x < 1 || dimensions.y < 1 || dimensions.z < 1)
            {
                throw new InternalLogicException("Dimension can't be a non-positive number");
            }

            var layer = EuclideanStateLayer<TQ>.GetOrCreate(machine.State);
            layer.FieldGrid = new Guid[dimensions.x, dimensions.y, dimensions.z];

            for (int z = 0; z < dimensions.z; z++)
            {
                for (int y = 0; y < dimensions.y; y++)
                {
                    for (int x = 0; x < dimensions.x; x++)
                    {
                        string name = $"{x}x{y}x{z}";

                        QVariable<TQ> newVariable = defaultValue.HasValue
                            ? QVariable<TQ>.New(name, defaultValue.Value)
                            : QVariable<TQ>.Empty(name);

                        DomainLayer<TQ>.GetOrCreate(newVariable).AssignDomain(domain);
                        EuclideanLayer<TQ>.GetOrCreate(newVariable).Update(x, y, z);

                        layer.FieldGrid[x, y, z] = newVariable.Id;
                        variables.Add(newVariable);
                    }
                }
            }

            machine.InitializeField(variables);
        }
    }
}
