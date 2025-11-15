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

            HashSet<string> topConn = new() { "╬", "║", "╗", "╔", "╣", "╠", "╦" };
            HashSet<string> topWall = new() { "═", "╚", "╝", "╩", " " };

            HashSet<string> bottomConn = new() { "╬", "║", "╚", "╝", "╣", "╠", "╩" };
            HashSet<string> bottomWall = new() { "╔", "╗", "═", "╦", " " };

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
                        Left = left,
                        Right = right,
                        Front = front,
                        Back = back,
                    }))
                    .Build();
            }

            mazeRules.Add(CreateRule("╬", leftConn, rightConn, topConn, bottomConn));
            mazeRules.Add(CreateRule("║", leftWall, rightWall, topConn, bottomConn));
            mazeRules.Add(CreateRule("═", leftConn, rightConn, topWall, bottomWall));
            mazeRules.Add(CreateRule("╔", leftWall, rightConn, topWall, bottomConn));
            mazeRules.Add(CreateRule("╗", leftConn, rightWall, topWall, bottomConn));
            mazeRules.Add(CreateRule("╚", leftWall, rightConn, topConn, bottomWall));
            mazeRules.Add(CreateRule("╝", leftConn, rightWall, topConn, bottomWall));
            mazeRules.Add(CreateRule("╠", leftWall, rightConn, topConn, bottomConn));
            mazeRules.Add(CreateRule("╣", leftConn, rightWall, topConn, bottomConn));
            mazeRules.Add(CreateRule("╦", leftConn, rightConn, topWall, bottomConn));
            mazeRules.Add(CreateRule("╩", leftConn, rightConn, topConn, bottomWall));
            mazeRules.Add(CreateRule(" ", leftWall, rightWall, topWall, bottomWall));

            QMachine<string> machine = new(new QMachineParameter<string>(){Random = new Random(10) });

            ConstraintLayer<string>.GetOrCreate(machine.State).Constraints = new()
            {
                GeneralConstraints = mazeRules
            };

            machine.GenerateField(new DiscreteDomain<string>(domain), (25, 25, 1));

            foreach (var variable in machine.State.Field)
            {
                if (DomainLayer<string>.With(variable).TryGetDomain<DiscreteDomain<string>>(out var localDomain))
                {
                    localDomain.UpdateWeight(" ", 21);
                    localDomain.UpdateWeight("╣", 51);
                }
            }

            foreach (var state in machine.States)
            {
                Console.Clear();
                GridPrinter.Print(state, 25);
            }
        }
    }
}
