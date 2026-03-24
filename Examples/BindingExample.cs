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

            var domain = DomainHelper.NumericalDomain((-100, 100));

            var a = machine.Q().WithDomain(domain);
            var b = machine.Q().WithDomain(domain);
            var c = machine.Q().WithDomain(domain);

            machine.AddConstraint(QSL.CreateConstraint<int>()
                .Bind(a, b, (_a, _b) => _a * _a + 1  == 2*_b*_b)
                .Build());

            machine.AddConstraint(QSL.CreateConstraint<int>()
                .Bind(a, c, (_a, _c) => _a + 1 == 2 * _c * _c)
                .Build());


            foreach (var state in machine.States)
            {
                Console.WriteLine($"{machine.Status}: {a.Display()}, {b.Display()} {c.Display()}");
            }

            //machine.AddConstraint(QSL.CreateConstraint<int>()
            //    .Select(QSL.Filters.All<int>())
            //    .Propagate(QSL.Propagators.AllDistinct<int>())
            //    .Build());

            //machine.AddConstraint(QSL.CreateConstraint<int>()
            //    .Bind(d, e, c0, y, (d_, e_, c0_, y_) => d_+e_ == (10 * c0_ + y_))
            //    .Build());

            //machine.AddConstraint(QSL.CreateConstraint<int>()
            //    .Bind(n, r, c0, c1, e, (_n, _r, _c0, _c1, _e) => _n + _r + _c0 == (10 * _c1 + _e))
            //    .Build());

            //machine.AddConstraint(QSL.CreateConstraint<int>()
            //    .Bind(e, o, c1, c2, n, (_e, _o, _c1, _c2, _n) => _e + _o + _c1 == (10 * _c2 + _n))
            //    .Build());

            //machine.AddConstraint(QSL.CreateConstraint<int>()
            //    .Bind(s, m, c2, c3, o, (_s, _m, _c2, _c3, _o) => _s + _o + _c2 == (10 * _c3 + _o))
            //    .Build());

            //machine.AddConstraint(QSL.CreateConstraint<int>()
            //    .Bind(c3, m, (_c3, _m) => _c3 == _m)
            //    .Build());
        }

        internal static string Display<TQ>(this QLink<TQ> link)
        {
            if (link.Variable.Value.HasValue)
            {
                return link.Variable.Value.Value.ToString();
            }


            return DomainLayer<TQ>.With(link.Variable).DescribeDomain();
        }
    }
}
