using qon;
using qon.Domains;
using qon.Functions.Constraints;
using qon.Functions.DSL;
using qon.Functions.Filters;
using qon.Helpers;
using qon.Machines;
using qon.Solvers;
using qon.Variables;
using System;
using System.Collections.Generic;

namespace Examples
{
    internal static class RotationExample
    {
        public static void Run(int size)
        {
            EuclideanBlockTemplate<string> tileGrassCube = new("Tile_Grass_cube");
            tileGrassCube.Add(Side.Left, "Open");
            tileGrassCube.Add(Side.Front, "Open");
            tileGrassCube.Add(Side.Right, "Open");
            tileGrassCube.Add(Side.Back, "Open");

            EuclideanBlockTemplate<string> cliffCornerInside = new("Cliff_corner_inside");
            cliffCornerInside.Add(Side.Left, "Open");
            cliffCornerInside.Add(Side.Front, "Open");
            cliffCornerInside.Add(Side.Right, "Closed");
            cliffCornerInside.Add(Side.Back, "Closed");

            EuclideanBlockTemplate<string> cliffCornerOutside = new("Cliff_Corner_outside");
            cliffCornerOutside.Add(Side.Left, "Closed");
            cliffCornerOutside.Add(Side.Front, "Closed");
            cliffCornerOutside.Add(Side.Right, "Open");
            cliffCornerOutside.Add(Side.Back, "Open");

            var blocks = EuclideanRotationHelper.GenerateConnections<string>(new List<EuclideanBlockTemplate<string>>
            {
                tileGrassCube,
                cliffCornerInside,
                cliffCornerOutside
            });

            List<EuclideanBlock<string>> domain = new();
            List<IQConstraint<EuclideanBlock<string>>> rotationRules = new();

            foreach (var block in blocks)
            {
                domain.Add(block.Key);
                rotationRules.Add(
                    QSL.Constraint<EuclideanBlock<string>>()
                        .When(Filters.EqualsToValue(block.Key))
                        .Where(QSL.VonNeumann(block.Value))
                        .Build());
            }

            var parameters = new WFCParameter<EuclideanBlock<string>>()
            {
                Constraints = new()
                {
                    GeneralConstraints = rotationRules
                },
                Random = new Random(100)
            };

            var machine = new WFCMachine<EuclideanBlock<string>>(parameters, m => new DefaultSolver<EuclideanBlock<string>>(m));

            machine.CreateEuclideanSpace((size, size, 1), new DiscreteDomain<EuclideanBlock<string>>(domain));
            int i = 0;
            foreach (var state in machine.States)
            {
                if (i % 5 == 0)
                {
                    Console.WriteLine($"{i}:{machine.State.CurrentState}");
                }

                i++;
            }
        }
    }
}
