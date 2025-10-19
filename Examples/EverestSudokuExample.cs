using qon;
using qon.Domains;
using qon.Functions.Constraints;
using qon.Functions.DSL;
using qon.Functions.Filters;
using qon.Functions.Propagators;
using qon.Helpers;
using qon.Layers.StateLayers;
using qon.Layers.VariableLayers;
using qon.Machines;
using qon.Solvers;
using qon.Variables;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Examples
{
    internal static class EverestSudokuExample
    {
        public static void Run()
        {
            List<int> domain = new() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            var parameters = new WFCParameter<int>()
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

            var machine = new WFCMachine<int>(parameters, m => new FiniteSolver<int>(m));
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
            var grid = EuclideanStateLayer<int>.With(machine.State);

            ConstraintLayer<int>.Collapse(grid[(0, 0, 0)]!, 8, true);

            ConstraintLayer<int>.Collapse(grid[(2, 1, 0)]!, 3, true);
            ConstraintLayer<int>.Collapse(grid[(3, 1, 0)]!, 6, true);

            ConstraintLayer<int>.Collapse(grid[(1, 2, 0)]!, 7, true);
            ConstraintLayer<int>.Collapse(grid[(4, 2, 0)]!, 9, true);
            ConstraintLayer<int>.Collapse(grid[(6, 2, 0)]!, 2, true);

            ConstraintLayer<int>.Collapse(grid[(1, 3, 0)]!, 5, true);
            ConstraintLayer<int>.Collapse(grid[(5, 3, 0)]!, 7, true);

            ConstraintLayer<int>.Collapse(grid[(4, 4, 0)]!, 4, true);
            ConstraintLayer<int>.Collapse(grid[(5, 4, 0)]!, 5, true);
            ConstraintLayer<int>.Collapse(grid[(6, 4, 0)]!, 7, true);

            ConstraintLayer<int>.Collapse(grid[(3, 5, 0)]!, 1, true);
            ConstraintLayer<int>.Collapse(grid[(7, 5, 0)]!, 3, true);

            ConstraintLayer<int>.Collapse(grid[(2, 6, 0)]!, 1, true);
            ConstraintLayer<int>.Collapse(grid[(7, 6, 0)]!, 6, true);
            ConstraintLayer<int>.Collapse(grid[(8, 6, 0)]!, 8, true);

            ConstraintLayer<int>.Collapse(grid[(2, 7, 0)]!, 8, true);
            ConstraintLayer<int>.Collapse(grid[(3, 7, 0)]!, 5, true);
            ConstraintLayer<int>.Collapse(grid[(7, 7, 0)]!, 1, true);

            ConstraintLayer<int>.Collapse(grid[(1, 8, 0)]!, 9, true);
            ConstraintLayer<int>.Collapse(grid[(6, 8, 0)]!, 4, true);
        }
    }
}
