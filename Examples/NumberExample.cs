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
using qon.QSL;

namespace Examples
{
    internal static class NumberExample
    {
        public static void Run()
        {
            var machine = new QMachine<int>(new QMachineParameter<int>());

            ConstraintLayer<int>.GetOrCreate(machine.State).Constraints = new()
            {
                GeneralConstraints = new()
                {
                    QSL.Constraint<int>()
                        .Select(Filters.All<int>())
                        .Propagate(Propagators.AllDistinct<int>())
                        .Build()
                }
            };

            var domain = new NumericalDomain<int>();

            var z = machine.Q();

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
