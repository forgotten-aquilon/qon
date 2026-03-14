using System;
using System.Collections.Generic;
using qon;
using qon.Functions.Constraints;
using qon.Functions.QSL;
using qon.Functions.Filters;
using qon.Functions.Propagators;
using qon.Layers.VariableLayers;
using qon.Machines;
using qon.Solvers;
using qon.Variables;
using qon.Functions;
using qon.Layers.StateLayers;
using qon.Variables.Domains;

namespace Examples
{
    internal static class MazeExample
    {
        public static void Run()
        {
            List<string> domain = new() { "╬", "║", "═", "╔", "╗", "╚", "╝", "╠", "╣", "╩", "╦", " " };

            HashSet<string> leftConn = new() { "╬", "═", "╔", "╚", "╠", "╩", "╦" };
            HashSet<string> leftWall = new() { "║", "╗", "╝", "╣", " " };

            HashSet<string> rightConn = new() { "╬", "═", "╗", "╝", "╣", "╩", "╦" };
            HashSet<string> rightWall = new() { "║", "╚", "╔", "╠", " " };

            HashSet<string> frontConn = new() { "╬", "║", "╗", "╔", "╣", "╠", "╦" };
            HashSet<string> frontWall = new() { "═", "╚", "╝", "╩", " " };

            HashSet<string> backConn = new() { "╬", "║", "╚", "╝", "╣", "╠", "╩" };
            HashSet<string> backWall = new() { "╔", "╗", "═", "╦", " " };

            List<IPreparation<string>> mazeRules = new();

            static IPreparation<string> CreateRule(
                string tile,
                HashSet<string> left,
                HashSet<string> right,
                HashSet<string> front,
                HashSet<string> back)
            {
                return QSL.Constraint<string>()
                    .When(Filters.EqualsToValue(tile))
                    .Where(QSL.VonNeumann(new EuclideanConstraintParameter<string>()
                    {
                        CenterLevel = {Left = left, Right = right, Front = front, Back = back},
                    }))
                    .Build();
            }

            mazeRules.Add(CreateRule("╬", leftConn, rightConn, frontConn, backConn));
            mazeRules.Add(CreateRule("║", leftWall, rightWall, frontConn, backConn));
            mazeRules.Add(CreateRule("═", leftConn, rightConn, frontWall, backWall));
            mazeRules.Add(CreateRule("╔", leftWall, rightConn, frontWall, backConn));
            mazeRules.Add(CreateRule("╗", leftConn, rightWall, frontWall, backConn));
            mazeRules.Add(CreateRule("╚", leftWall, rightConn, frontConn, backWall));
            mazeRules.Add(CreateRule("╝", leftConn, rightWall, frontConn, backWall));
            mazeRules.Add(CreateRule("╠", leftWall, rightConn, frontConn, backConn));
            mazeRules.Add(CreateRule("╣", leftConn, rightWall, frontConn, backConn));
            mazeRules.Add(CreateRule("╦", leftConn, rightConn, frontWall, backConn));
            mazeRules.Add(CreateRule("╩", leftConn, rightConn, frontConn, backWall));
            mazeRules.Add(CreateRule(" ", leftWall, rightWall, frontWall, backWall));

            QMachine<string> machine = new(new QMachineParameter<string>(){Random = new Random(10) });

            ConstraintLayer<string>.GetOrCreate(machine.State).Constraints = new()
            {
                GeneralConstraints = mazeRules
            };

            var d = new DiscreteDomain<string>(domain);
            d.UpdateWeight(" ", 21);
            d.UpdateWeight("╣", 51);

            machine.GenerateField(d, (25, 25, 1));

            foreach (var state in machine.States)
            {
                Console.Clear();
                GridPrinter.Print(state, 25);
            }
        }
    }
}
