using qon.Domains;
using qon.Exceptions;
using qon.Layers.StateLayers;
using qon.Layers.VariableLayers;
using qon.Variables;
using System;
using System.Collections.Generic;

namespace qon.Machines
{
    public enum FieldType
    {
        Common,
        Euclidean,
        Hexagonal
    }

    public class WFCMachine<T> : QMachine<T>
    {

        public FieldType FieldType { get; private set; }

        public WFCMachine(WFCParameter<T> parameter, Func<QMachine<T>, IEnumerator<MachineState<T>>> factory) : base(parameter, factory)
        {   
            ConstraintLayer<T>.GetOrCreate(State).Constraints = parameter.Constraints;
        }

        public void CreateEuclideanSpace((int x, int y, int z) dimensions, IDomain<T> domain)
        {
            FieldType = FieldType.Euclidean;
                
            List<QVariable<T>> variables = new();

            if (dimensions.x < 1 || dimensions.y < 1 || dimensions.z < 1)
            {
                throw new InternalLogicException("Dimension can't be a non-positive number");
            }

            var layer = EuclideanStateLayer<T>.GetOrCreate(State);
            layer.FieldGrid = new string[dimensions.x, dimensions.y, dimensions.z];

            for (int x = 0; x < dimensions.x; x++)
            {
                for (int y = 0; y < dimensions.y; y++)
                {
                    for (int z = 0; z < dimensions.z; z++)
                    {
                        string name = $"{x}x{y}x{z}";
                        var v = new QVariable<T>(name);
                        DomainLayer<T>.GetOrCreate(v).AssignDomain(domain);
                        EuclideanLayer<T>.GetOrCreate(v).Update(x, y, z);

                        layer.FieldGrid[x, y, z] = name;

                        variables.Add(v);
                    }
                }
            }

            SetField(variables);
        }
    }
}
