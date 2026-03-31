using qon;
using qon.Functions;
using qon.Functions.Constraints;
using qon.Functions.Filters;
using qon.Helpers;
using qon.Machines;
using qon.Variables;
using qon.Variables.Domains;
using System;
using System.Collections.Generic;

namespace Examples
{
    internal static class RotationExample
    {
        public static void Run(int size)
        {
            EuclideanBlockTemplate<string> tileGrassCube = new("Tile_Grass_cube");
            tileGrassCube.Add(Level.Middle, Side.Left, "Open");
            tileGrassCube.Add(Level.Middle, Side.Front, "Open");
            tileGrassCube.Add(Level.Middle, Side.Right, "Open");
            tileGrassCube.Add(Level.Middle, Side.Back, "Open");

            EuclideanBlockTemplate<string> cliffCornerInside = new("Cliff_corner_inside");
            cliffCornerInside.Add(Level.Middle, Side.Left, "Open");
            cliffCornerInside.Add(Level.Middle, Side.Front, "Open");
            cliffCornerInside.Add(Level.Middle, Side.Right, "Closed");
            cliffCornerInside.Add(Level.Middle, Side.Back, "Closed");

            EuclideanBlockTemplate<string> cliffCornerOutside = new("Cliff_Corner_outside");
            cliffCornerOutside.Add(Level.Middle, Side.Left, "Closed");
            cliffCornerOutside.Add(Level.Middle, Side.Front, "Closed");
            cliffCornerOutside.Add(Level.Middle, Side.Right, "Open");
            cliffCornerOutside.Add(Level.Middle, Side.Back, "Open");

            var blocks = EuclideanRotationHelper.GenerateConnections<string>(new List<EuclideanBlockTemplate<string>>
            {
                tileGrassCube,
                cliffCornerInside,
                cliffCornerOutside
            });

            List<EuclideanBlock<string>> domain = new();
            List<IPreparation<EuclideanBlock<string>>> rotationRules = new();

            foreach (var block in blocks)
            {
                domain.Add(block.Key);
                rotationRules.Add(
                    QSL.CreateConstraint<EuclideanBlock<string>>()
                        .When(QSL.Filters.EqualsToValue(block.Key))
                        .Where(QSL.VonNeumann(block.Value))
                        .Build());
            }

            var machine = QSL.Machine<EuclideanBlock<string>>(new QMachineParameter<EuclideanBlock<string>>() { Random = new Random(100) })
                .WithConstraintLayer(new()
                {
                    GeneralConstraints = rotationRules
                })
                .GenerateField((size, size, 1), new DiscreteDomain<EuclideanBlock<string>>(domain));

            int i = 0;
            foreach (var state in machine.States)
            {
                if (i % 5 == 0)
                {
                    Console.WriteLine($"{i}");
                }

                i++;
            }
        }
    }
}
