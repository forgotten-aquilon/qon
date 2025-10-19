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
        protected string[,,] FieldGrid {  get; set; } = new string[0, 0, 0];

        public FieldType FieldType { get; private set; }

        public WFCMachine(WFCParameter<T> parameter, Func<QMachine<T>, IEnumerator<MachineState<T>>> factory) : base(parameter, factory)
        {
            ConstraintLayer<T>.TryCreate(State).Constraints = parameter.Constraints;
        }

        public void CreateEuclideanSpace((int x, int y, int z) dimensions, IDomain<T> domain)
        {
            FieldType = FieldType.Euclidean;
                
            List<QVariable<T>> variables = new();

            if (dimensions.x < 1 || dimensions.y < 1 || dimensions.z < 1)
            {
                throw new InternalLogicException("Dimension can't be a non-positive number");
            }

            FieldGrid = new string[dimensions.x, dimensions.y, dimensions.z];
            var layer = EuclideanStateLayer<T>.TryCreate(State);
            layer.FieldGrid = FieldGrid;
            layer.Machine = this;

            for (int x = 0; x < dimensions.x; x++)
            {
                for (int y = 0; y < dimensions.y; y++)
                {
                    for (int z = 0; z < dimensions.z; z++)
                    {
                        string name = $"{x}x{y}x{z}";
                        var v = new QVariable<T>(name);
                        DomainLayer<T>.TryCreate(v).Domain = domain;
                        v.Layers.Add(new EuclideanLayer<T>(x, y, z, this));

                        FieldGrid[x, y, z] = name;
                        EuclideanStateLayer<T>.With(State).FieldGrid[x, y, z] = name;

                        variables.Add(v);
                    }
                }
            }

            SetField(variables);
        }

    }
}
