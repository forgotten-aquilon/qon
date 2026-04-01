using System;
using System.Collections.Generic;
using System.Linq;
using qon;
using qon.Functions.Filters;
using qon.Functions.Propagators;
using qon.Helpers;
using qon.Layers.StateLayers;
using qon.Layers.VariableLayers;
using qon.Machines;
using qon.QSL;
using qon.Solvers;
using qon.Variables;
using qon.Variables.Domains;

namespace Examples
{
    internal static class EightQueensExample
    {
        public static void Run()
        {
            var domain = new DiscreteDomain<char>(new List<char>() { 'Q', '.' });

            var machine = QMachine<char>.Create(new()
            {
                Random = new Random(2),  
            })
                .WithConstraintLayer(new()
                {
                    GeneralConstraints = new()
                    {
                        //TODO: Add new QSL functions for layers
                        Constraints.CreateConstraint<char>()
                            .When(Filters.EqualsToValue('Q'))
                            //.Where(Cartesian.OnLayer<char>((l1, l2) => l1.X == l2.X || l1.Y == l2.Y || Math.Abs(l1.X - l2.X) == Math.Abs(l1.Y - l2.Y)))
                            .Where(v1 => new QPredicate<char>(v2 =>
                            {
                                var v1e = v1.Cartesian();
                                var v2e = v2.Cartesian();

                                return v1e.X == v2e.X || v1e.Y == v2e.Y || Math.Abs(v1e.X - v2e.X) == Math.Abs(v1e.Y - v2e.Y);
                            }))
                            .Propagate(Propagators.ReduceDomainTo(new HashSet<char> { '.' }))
                            .Build()
                    },
                    ValidationConstraints = new()
                    {
                        Constraints.CreateConstraint<char>()
                            .Constraint(field => (field.Count(Filters.EqualsToValue('Q')) == 8).Then(Propagators.FromBool(true)))
                            .Build(),
                    }
                })
                .GenerateField((8, 8, 1), domain);


            foreach (var state in machine.States)
            {
                Console.Clear();
                GridPrinter.Print(state, 8, true);
                Console.WriteLine($"{machine.Solver.StepCounter} {machine.Status}");
            }
        }
    }
}
