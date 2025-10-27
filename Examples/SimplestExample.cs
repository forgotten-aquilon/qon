using System;
using System.Collections.Generic;
using qon;
using qon.Domains;
using qon.Functions.QSL;
using qon.Functions.Filters;
using qon.Functions.Propagators;
using qon.Machines;
using qon.Solvers;

namespace Examples
{
    internal static class SimplestExample
    {
        public static void Run()
        {
            var parameter = new WFCParameter<char>()
            {
                Constraints = new()
                {
                    GeneralConstraints = new()
                    {
                        QSL.Constraint<char>()
                            .Select(Filters.All<char>())
                            .Propagate(Propagators.AllDistinct<char>())
                            .Build()
                    }
                }
            };

            var machine = new QMachine<char>(parameter, m => new DefaultSolver<char>(m));

            List<char> letters = new() { 'a', 'b', 'c', 'd', 'e', 'f', 'g' };
            machine.GenerateField(new DiscreteDomain<char>(letters), new[] { "V1", "V2", "V3", "V4" });

            foreach (var state in machine.States)
            {
                Console.WriteLine(state);
            }
        }
    }
}
