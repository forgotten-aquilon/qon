using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    internal static class EverestSudokuExample
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
                            .GroupBy(EuclideanFilters.GroupByRectangle<int>(3, 3))
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
                Random = new Random(2222)
            };

            var machine = new WFCMachine<int>(parameters);
            machine.CreateEuclideanSpace((9, 9, 1), new DiscreteDomain<int>(domain));

            SeedField(machine);

            int step = 0;

            foreach (var state in machine.States)
            {
                Console.Clear();
                SudokuPrinter.Print(state, 9);
                Console.WriteLine(step);
                step++;

                Task.Run(async () =>
                {
                    await Task.Delay(0);
                }).GetAwaiter().GetResult();
            }
        }

        private static void SeedField(WFCMachine<int> machine)
        {
            SuperpositionLayer<int>.Collapse(machine[(0, 0, 0)]!, 8, true);

            SuperpositionLayer<int>.Collapse(machine[(2, 1, 0)]!, 3, true);
            SuperpositionLayer<int>.Collapse(machine[(3, 1, 0)]!, 6, true);

            SuperpositionLayer<int>.Collapse(machine[(1, 2, 0)]!, 7, true);
            SuperpositionLayer<int>.Collapse(machine[(4, 2, 0)]!, 9, true);
            SuperpositionLayer<int>.Collapse(machine[(6, 2, 0)]!, 2, true);

            SuperpositionLayer<int>.Collapse(machine[(1, 3, 0)]!, 5, true);
            SuperpositionLayer<int>.Collapse(machine[(5, 3, 0)]!, 7, true);

            SuperpositionLayer<int>.Collapse(machine[(4, 4, 0)]!, 4, true);
            SuperpositionLayer<int>.Collapse(machine[(5, 4, 0)]!, 5, true);
            SuperpositionLayer<int>.Collapse(machine[(6, 4, 0)]!, 7, true);

            SuperpositionLayer<int>.Collapse(machine[(3, 5, 0)]!, 1, true);
            SuperpositionLayer<int>.Collapse(machine[(7, 5, 0)]!, 3, true);

            SuperpositionLayer<int>.Collapse(machine[(2, 6, 0)]!, 1, true);
            SuperpositionLayer<int>.Collapse(machine[(7, 6, 0)]!, 6, true);
            SuperpositionLayer<int>.Collapse(machine[(8, 6, 0)]!, 8, true);

            SuperpositionLayer<int>.Collapse(machine[(2, 7, 0)]!, 8, true);
            SuperpositionLayer<int>.Collapse(machine[(3, 7, 0)]!, 5, true);
            SuperpositionLayer<int>.Collapse(machine[(7, 7, 0)]!, 1, true);

            SuperpositionLayer<int>.Collapse(machine[(1, 8, 0)]!, 9, true);
            SuperpositionLayer<int>.Collapse(machine[(6, 8, 0)]!, 4, true);
        }
    }
}
