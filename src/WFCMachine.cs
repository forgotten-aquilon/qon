using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using qon.Domains;
using qon.Exceptions;
using qon.Rules;
using qon.Variables;

namespace qon
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

        public WFCMachine(QMachineParameter<T> parameter) : base(parameter)
        {
        }

        public void CreateEuclideanSpace((int x, int y, int z) dimensions, IDomain<T> domain)
        {
            FieldType = FieldType.Euclidean;
                
            List<SuperpositionVariable<T>> variables = new();

            if (dimensions.x < 1 || dimensions.y < 1 || dimensions.z < 1)
            {
                throw new InternalLogicException("Dimension can't be a non-positive number");
            }

            FieldGrid = new string[dimensions.x, dimensions.y, dimensions.z];

            for (int x = 0; x < dimensions.x; x++)
            {
                for (int y = 0; y < dimensions.y; y++)
                {
                    for (int z = 0; z < dimensions.z; z++)
                    {
                        string name = $"{x}x{y}x{z}";
                        var v = new EuclideanVariable<T>(this, domain, name)
                        {
                            X = x,
                            Y = y, 
                            Z = z
                        };

                        FieldGrid[x, y, z] = name;

                        variables.Add(v);
                    }
                }
            }

            SetField(variables);
        }

        public SuperpositionVariable<T>? this[int x, int y, int z]
            => State["x", x]["y", y]["z", z].Result.FirstOrDefault();

        public SuperpositionVariable<T>? this[int x, int y]
            => State["x", x]["y", y]["z", 0].Result.FirstOrDefault();

        public SuperpositionVariable<T>? this[int x]
            => State["x", x]["y", 0]["z", 0].Result.FirstOrDefault();

        public SuperpositionVariable<T>? this[(int x, int y, int z) coordinate]
        {
            get
            {
                if (coordinate.x < 0 || coordinate.y < 0 || coordinate.z < 0)
                {
                    return null;
                }
                
                if (coordinate.x >= FieldGrid.GetLength(0) || coordinate.y >= FieldGrid.GetLength(1) || coordinate.z >= FieldGrid.GetLength(2))
                {
                    return null;
                }

                return this[FieldGrid[coordinate.x, coordinate.y, coordinate.z]];
            }
        }

    }
}
