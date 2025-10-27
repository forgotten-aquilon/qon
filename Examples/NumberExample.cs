using System;
using qon;
using qon.Domains;
using qon.Functions.QSL;
using qon.Functions.Filters;
using qon.Functions.Propagators;
using qon.Machines;
using qon.Solvers;

namespace Examples
{
    internal static class NumberExample
    {
        public static void Run()
        {
            var parameter = new WFCParameter<int>()
            {
                Constraints = new()
                {
                    GeneralConstraints = new()
                    {
                        QSL.Constraint<int>()
                            .Select(Filters.All<int>())
                            .Propagate(Propagators.AllDistinct<int>())
                            .Build()
                    }
                }
            };

            var machine = new QMachine<int>(parameter, m => new DefaultSolver<int>(m));

            machine.GenerateField(new NumericalDomain<int>(), new[] { "V1", "V2", "V3", "V4" });

            foreach (var state in machine.States)
            {
                Console.WriteLine(state);
            }
        }
    }
}
