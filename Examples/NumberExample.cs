using System;
using System.Collections.Generic;
using qon;
using qon.Functions.Filters;
using qon.Functions.Propagators;
using qon.Helpers;
using qon.Machines;
using qon.Solvers;
using qon.Variables.Domains;
using qon.Layers.StateLayers;

namespace Examples
{
    internal static class NumberExample
    {
        public static void Run()
        {
            var machine = QSL.Machine<int>().WithConstraintLayer(new()
            {
                GeneralConstraints = new()
                {
                    QSL.CreateConstraint<int>()
                        .Select(QSL.Filters.All<int>())
                        .Propagate(QSL.Propagators.AllDistinct<int>())
                        .Build()
                }
            });

            var domain = new NumericalDomain<int>();

            var a = machine.Q().WithDomain(domain);
            var b = machine.Q().WithDomain(domain);
            var c = machine.Q().WithDomain(domain);
            var d = machine.Q().WithDomain(domain);

            foreach (var state in machine.States)
            {
                Console.WriteLine($"{machine.Status}: {a}, {b}, {c}, {d}");
            }
        }
    }
}
