using System;
using System.Collections.Generic;
using qon;
using qon.Domains;
using qon.Functions.Constraints;
using qon.Functions.DSL;
using qon.Functions.Filters;
using qon.Functions.Propagators;
using qon.Helpers;
using qon.Layers.VariableLayers;
using qon.Variables;

namespace Examples
{
    internal static class SudokuExample
    {
        public static void Run()
        {
            List<int> domain = new() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            var parameters = new QMachineParameter<int>()
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
                }
            };

            var machine = new WFCMachine<int>(parameters);
            var numericalDomain = new NumericalDomain<int>(new List<Interval<int>>() { new Interval<int>(1, 9) });
            machine.CreateEuclideanSpace((9, 9, 1), numericalDomain);

            SeedField(machine);

            int step = 0;

            foreach (var state in machine.States)
            {
                Console.Clear();
                SudokuPrinter.Print(state, 9);
                Console.WriteLine(step);
                step++;
            }
        }

        private static void SeedField(WFCMachine<int> machine)
        {
            SuperpositionLayer<int>.Collapse(machine[(0, 0, 0)]!, 9, true);
            SuperpositionLayer<int>.Collapse(machine[(1, 0, 0)]!, 5, true);
            SuperpositionLayer<int>.Collapse(machine[(3, 0, 0)]!, 2, true);
            SuperpositionLayer<int>.Collapse(machine[(6, 0, 0)]!, 7, true);

            SuperpositionLayer<int>.Collapse(machine[(4, 1, 0)]!, 6, true);
            SuperpositionLayer<int>.Collapse(machine[(5, 1, 0)]!, 5, true);

            SuperpositionLayer<int>.Collapse(machine[(1, 2, 0)]!, 6, true);
            SuperpositionLayer<int>.Collapse(machine[(5, 2, 0)]!, 9, true);
            SuperpositionLayer<int>.Collapse(machine[(6, 2, 0)]!, 2, true);

            SuperpositionLayer<int>.Collapse(machine[(3, 3, 0)]!, 4, true);
            SuperpositionLayer<int>.Collapse(machine[(5, 3, 0)]!, 7, true);
            SuperpositionLayer<int>.Collapse(machine[(7, 3, 0)]!, 6, true);
            SuperpositionLayer<int>.Collapse(machine[(8, 3, 0)]!, 3, true);

            SuperpositionLayer<int>.Collapse(machine[(0, 4, 0)]!, 2, true);
            SuperpositionLayer<int>.Collapse(machine[(7, 4, 0)]!, 7, true);

            SuperpositionLayer<int>.Collapse(machine[(2, 5, 0)]!, 3, true);

            SuperpositionLayer<int>.Collapse(machine[(0, 6, 0)]!, 7, true);
            SuperpositionLayer<int>.Collapse(machine[(1, 6, 0)]!, 3, true);
            SuperpositionLayer<int>.Collapse(machine[(3, 6, 0)]!, 5, true);
            SuperpositionLayer<int>.Collapse(machine[(8, 6, 0)]!, 1, true);

            SuperpositionLayer<int>.Collapse(machine[(0, 7, 0)]!, 8, true);
            SuperpositionLayer<int>.Collapse(machine[(5, 7, 0)]!, 6, true);

            SuperpositionLayer<int>.Collapse(machine[(0, 8, 0)]!, 1, true);
            SuperpositionLayer<int>.Collapse(machine[(2, 8, 0)]!, 6, true);
            SuperpositionLayer<int>.Collapse(machine[(4, 8, 0)]!, 4, true);
            SuperpositionLayer<int>.Collapse(machine[(8, 8, 0)]!, 8, true);
        }
    }
}
