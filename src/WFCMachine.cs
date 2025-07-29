using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using qon.Domains;
using qon.Rules;

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
        public FieldType FieldType { get; private set; }

        public WFCMachine(QMachineParameter<T> parameter) : base(parameter)
        {
        }

        public void CreateEuclideanSpace((int x, int y, int z) dimensions, IDomain<T>? domain = null)
        {
            if (dimensions.x == 0)
                throw new InternalLogicException("Dimension x should not be zero");

            if (dimensions.z != 0 && dimensions.y == 0)
                throw new InternalLogicException("Dimension y should not be zero if dimension z is non-zero");

            FieldType = FieldType.Euclidean;

            List<SuperpositionVariable<T>> variables = new();

            for (int x = 0; x < dimensions.x; x++)
            {
                for (int y = 0; y < dimensions.y; y++)
                {
                    for (int z = 0; z < dimensions.z; z++)
                    {
                        string name = $"{x}x{y}x{z}";
                        var v = new SuperpositionVariable<T>(domain, name)
                            .WithProperty("x", x)
                            .WithProperty("y", y)
                            .WithProperty("z", z);

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
            => State["x", x]["y", 1]["z", 0].Result.FirstOrDefault();
    }
}
