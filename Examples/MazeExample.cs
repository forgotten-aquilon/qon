using System;
using System.Collections.Generic;
using qon;
using qon.Domains;
using qon.Functions.Constraints;
using qon.Functions.DSL;
using qon.Functions.Filters;
using qon.Functions.Propagators;
using qon.Layers.VariableLayers;
using qon.Machines;
using qon.Solvers;
using qon.Variables;

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

            List<IQConstraint<string>> mazeRules = new();

            static IQConstraint<string> CreateRule(
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

            var parameters = new WFCParameter<string>()
            {
                Constraints = new()
                {
                    GeneralConstraints = mazeRules
                },
                Random = new Random(10)
            };

            WFCMachine<string> machine = new(parameters, m => new DefaultSolver<string>(m));

            machine.CreateEuclideanSpace((25, 25, 1), new DiscreteDomain<string>(domain));

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
