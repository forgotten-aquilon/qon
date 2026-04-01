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
using qon.QSL;

namespace Examples
{
    internal static class RotationExample
    {
        public static void Run(int size)
        {
            CartesianBlockTemplate<string> tileGrassCube = new("Tile_Grass_cube");
            tileGrassCube.Add(Level.Middle, Side.Left, "Open");
            tileGrassCube.Add(Level.Middle, Side.Front, "Open");
            tileGrassCube.Add(Level.Middle, Side.Right, "Open");
            tileGrassCube.Add(Level.Middle, Side.Back, "Open");

            CartesianBlockTemplate<string> cliffCornerInside = new("Cliff_corner_inside");
            cliffCornerInside.Add(Level.Middle, Side.Left, "Open");
            cliffCornerInside.Add(Level.Middle, Side.Front, "Open");
            cliffCornerInside.Add(Level.Middle, Side.Right, "Closed");
            cliffCornerInside.Add(Level.Middle, Side.Back, "Closed");

            CartesianBlockTemplate<string> cliffCornerOutside = new("Cliff_Corner_outside");
            cliffCornerOutside.Add(Level.Middle, Side.Left, "Closed");
            cliffCornerOutside.Add(Level.Middle, Side.Front, "Closed");
            cliffCornerOutside.Add(Level.Middle, Side.Right, "Open");
            cliffCornerOutside.Add(Level.Middle, Side.Back, "Open");

            var blocks = CartesianRotationHelper.GenerateConnections<string>(new List<CartesianBlockTemplate<string>>
            {
                tileGrassCube,
                cliffCornerInside,
                cliffCornerOutside
            });

            List<CartesianBlock<string>> domain = new();
            List<IPreparation<CartesianBlock<string>>> rotationRules = new();

            foreach (var block in blocks)
            {
                domain.Add(block.Key);
                rotationRules.Add(
                    Constraints.CreateConstraint<CartesianBlock<string>>()
                        .When(Filters.EqualsToValue(block.Key))
                        .Where(Cartesian.VonNeumann(block.Value))
                        .Build());
            }

            var machine = QMachine<CartesianBlock<string>>.Create(new QMachineParameter<CartesianBlock<string>>() { Random = new Random(100) })
                .WithConstraintLayer(new()
                {
                    GeneralConstraints = rotationRules
                })
                .GenerateField((size, size, 1), new DiscreteDomain<CartesianBlock<string>>(domain));

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
