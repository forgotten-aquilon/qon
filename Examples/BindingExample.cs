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
using qon.Layers.VariableLayers;
using qon.Variables;

namespace Examples
{
    internal static class BindingExample
    {
        public static void Run()
        {
            var machine = QSL.Machine<int>();

            var domain = DomainHelper.NumericalDomain((0, 9));

            var a = machine.Q().WithDomain(domain);
            var b = machine.Q().WithDomain(domain);
            var c = machine.Q().WithDomain(domain);

            machine.AddConstraint(QSL.CreateConstraint<int>()
                .Bind(a, b, (_a, _b) => _a * _a + 1 == 2 * _b * _b)
                .Build());

            machine.AddConstraint(QSL.CreateConstraint<int>()
                .Bind(a, c, (_a, _c) => _a + 1 == 2 * _c * _c)
                .Build());


            foreach (var state in machine.States)
            {
                Console.WriteLine($"{machine.Status}: {a.Display()}, {b.Display()}, {c.Display()}");
            }
        }

        internal static string Display<TQ>(this QLink<TQ> link)
        {
            return link.Variable.Value.HasValue ? link.Variable.Value.Value.ToString() : "N";
        }
    }
}
