using System;
using System.Collections.Generic;
using System.Linq;
using qon;
using qon.Functions.QSL;
using qon.Functions.Filters;
using qon.Functions.Operations;
using qon.Functions.Propagators;
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

            var machine = new QMachine<char>(new QMachineParameter<char>());

            ConstraintLayer<char>.GetOrCreate(machine.State).Constraints = new()
            {
                GeneralConstraints = new()
                {
                    QSL.Constraint<char>()
                        .When(Filters.EqualsToValue('Q'))
                        .Where(QSL.WithLayer<char, EuclideanLayer<char>>(l1 =>
                            QPredicate<char>.Create<EuclideanLayer<char>>(l2 => l1.X == l2.X)
                            | QPredicate<char>.Create<EuclideanLayer<char>>(l2 => l1.Y == l2.Y)
                            | QPredicate<char>.Create<EuclideanLayer<char>>(l2 =>
                                Math.Abs(l1.X - l2.X) == Math.Abs(l1.Y - l2.Y))))
                        .Propagate(Propagators.ReduceDomainTo<char>(new HashSet<char> { '.' }))
                        .Build()
                },
                ValidationConstraints = new()
                {
                    QSL.Constraint<char>()
                        .Execute(field =>
                            (field.Count(Filters.EqualsToValue('Q').ApplyTo) == 8)
                            + ~Propagators.FromBool(true))
                        .Build(),
                }
            };

            machine.GenerateField(domain, (8, 8, 1));

            foreach (var state in machine.States)
            {
                Console.Clear();
                GridPrinter.Print(state, 8, true);
            }
        }
    }
}
