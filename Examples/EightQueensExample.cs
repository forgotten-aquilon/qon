using System;
using System.Collections.Generic;
using System.Linq;
using qon;
using qon.Domains;
using qon.Functions.DSL;
using qon.Functions.Filters;
using qon.Functions.Operations;
using qon.Functions.Propagators;
using qon.Layers.VariableLayers;
using qon.Variables;

namespace Examples
{
    internal static class EightQueensExample
    {
        public static void Run()
        {
            var domain = new DiscreteDomain<char>(new List<char>() { 'Q', '.' });

            var parameters = new QMachineParameter<char>
            {
                Constraints = new()
                {
                    GeneralConstraints =  new()
                    {
                        QSL.Constraint<char>()
                            .When(Filters.EqualsToValue('Q'))
                            .Where(QSL.WithLayer<char, EuclideanLayer<char>>(
                                l1 =>
                                      QPredicate<char>.Create<EuclideanLayer<char>>(l2 => l1.X == l2.X)
                                    | QPredicate<char>.Create<EuclideanLayer<char>>(l2 => l1.Y == l2.Y)
                                    | QPredicate<char>.Create<EuclideanLayer<char>>(l2 => Math.Abs(l1.X - l2.X) == Math.Abs(l1.Y - l2.Y))))
                            .Propagate(Propagators.DomainIntersectionWithHashSet<char>(new HashSet<char> { '.' }))
                            .Build()
                    },
                    ValidationConstraints = new()
                    {
                        QSL.Constraint<char>()
                            .Execute(field =>
                                field.Where(Filters.EqualsToValue('Q').ApplyTo).Count()
                                + ~Operations.Comparison(8, COperator.EQ)
                                + ~Propagators.FromBool(true))
                            .Build(),
                    }
                }
            };

            var machine = new WFCMachine<char>(parameters);
            machine.CreateEuclideanSpace((8, 8, 1), domain);

            foreach (var state in machine.States)
            {
                Console.Clear();
                GridPrinter.Print(state, 8, true);
            }
        }
    }
}
