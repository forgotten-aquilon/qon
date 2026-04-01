using qon;
using qon.Variables.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Machines;
using qon.QSL;

namespace Examples
{
    public class SendMoreMoneyExample
    {
        public static void Run()
        {
            var machine = QMachine<int>.Create();

            var domain = DomainHelper.NumericalDomain((0, 9));

            var s = machine.Q().WithDomain(domain);
            var e = machine.Q().WithDomain(domain);
            var n = machine.Q().WithDomain(domain);
            var d = machine.Q().WithDomain(domain);
            var m = machine.Q().WithDomain(DomainHelper.NumericalDomain((1, 9)));
            var o = machine.Q().WithDomain(domain);
            var r = machine.Q().WithDomain(domain);
            var y = machine.Q().WithDomain(domain);

            machine.AddConstraint(Constraints.CreateConstraint<int>()
                .Select(Filters.All<int>())
                .Propagate(Propagators.AllDistinct<int>())
                .Build());

            machine.AddConstraint(Constraints.CreateConstraint<int>()
                .Bind(s, e, n, d, m, o, r, y, (_s, _e, _n, _d, _m, _o, _r, _y) =>
                    (_s * 1000 + _e * 100 + _n * 10 + _d) + (_m * 1000 + _o * 100 + _r * 10 + _e) == _m * 10000 + _o * 1000 + _n * 100 + _e * 10 + _y)
                .Build());

            foreach (var _ in machine.States)
            {
                Console.Clear();
                Console.WriteLine($"{machine.Status}, {machine.Solver.StepCounter}:{machine.Solver.BackStepCounter}");
                Console.WriteLine($" {s.Display()}{e.Display()}{n.Display()}{d.Display()}");
                Console.WriteLine($" {m.Display()}{o.Display()}{r.Display()}{e.Display()}");
                Console.WriteLine($"{m.Display()}{o.Display()}{n.Display()}{e.Display()}{y.Display()}");
            }
        }
    }
}
