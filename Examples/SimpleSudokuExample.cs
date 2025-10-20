using qon;
using qon.Domains;
using qon.Functions.Constraints;
using qon.Functions.DSL;
using qon.Functions.Filters;
using qon.Functions.Propagators;
using qon.Helpers;
using qon.Machines;
using qon.Solvers;
using System;
using System.Collections.Generic;

namespace Examples
{
    internal static class SimpleSudokuExample
    {
        public static void Run()
        {
            var parameters = new WFCParameter<int>()
            {
                Constraints = new()
                {
                    GeneralConstraints = new()
                    {
                        QSL.Constraint<int>()
                            .GroupBy(EuclideanFilters.GroupByRectangle<int>(2, 2))
                            .Propagate(Propagators.AllDistinct<int>())
                            .Build(),
                        QSL.Constraint<int>()
                            .GroupBy(EuclideanFilters.GroupByX<int>())
                            .Propagate(Propagators.AllDistinct<int>())
                            .Build(),
                        QSL.Constraint<int>()
                            .GroupBy(EuclideanFilters.GroupByY<int>())
                            .Propagate(Propagators.AllDistinct<int>())
                            .Build(),
                    }
                },
                Random = new Random(12)
            };

            var numericalDomain = new NumericalDomain<int>(new List<Interval<int>>() { new Interval<int>(1, 4) });
            var machine = new WFCMachine<int>(parameters, m => new DefaultSolver<int>(m));
            machine.CreateEuclideanSpace((4, 4, 1), numericalDomain);

            int step = 0;

            foreach (var state in machine.States)
            {
                Console.Clear();
                SudokuPrinter.Print(state, 4);
                Console.WriteLine(step);
                step++;
            }
        }
    }
}
