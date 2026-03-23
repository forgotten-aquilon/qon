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

            var machine = QSL.Machine<char>(new()
            {
                Random = new Random(7),  
            })
                .WithConstraint(new()
                {
                    GeneralConstraints = new()
                    {
                        QSL.CreateConstraint<char>()
                            .When(QSL.Filters.EqualsToValue('Q'))
                            .Where(QSL.OnLayer<char, EuclideanLayer<char>>(l1 =>
                                QPredicate<char>.Create<EuclideanLayer<char>>(l2 => l1.X == l2.X)
                                | QPredicate<char>.Create<EuclideanLayer<char>>(l2 => l1.Y == l2.Y)
                                | QPredicate<char>.Create<EuclideanLayer<char>>(l2 =>
                                    Math.Abs(l1.X - l2.X) == Math.Abs(l1.Y - l2.Y))))
                            .Propagate(QSL.Propagators.ReduceDomainTo<char>(new HashSet<char> { '.' }))
                            .Build()
                    },
                    ValidationConstraints = new()
                    {
                        QSL.CreateConstraint<char>()
                            .Execute(field => (field.Count(QSL.Filters.EqualsToValue('Q')) == 8).Then(QSL.Propagators.FromBool(true)))
                            .Build(),
                    }
                })
                .GenerateField(domain, (8, 8, 1));


            foreach (var state in machine.States)
            {
                Console.Clear();
                GridPrinter.Print(state, 8, true);
                Console.WriteLine($"{machine.Solver.StepCounter} {machine.Status}");
            }
        }
    }
}
