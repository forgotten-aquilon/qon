using qon;
using qon.Functions.Constraints;
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
using qon.Variables.Domains;

namespace Examples
{
    internal static class SudokuExample
    {
        public static void Run()
        {
            List<int> domain = new() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            var machine = new QMachine<int>(new QMachineParameter<int>()
            {
                Random = new Random(1),
            });
            ConstraintLayer<int>.GetOrCreate(machine.State).Constraints = new()
            {
                GeneralConstraints = new()
                {
                    QSL.CreateConstraint<int>()
                        .GroupBy(EuclideanFilters.GroupByRectangle<int>(3, 3))
                        .Propagate(Propagators.AllDistinct<int>())
                        .Build(),
                    QSL.CreateConstraint<int>()
                        .GroupBy(EuclideanFilters.GroupByX<int>())
                        .Propagate(Propagators.AllDistinct<int>())
                        .Build(),
                    QSL.CreateConstraint<int>()
                        .GroupBy(EuclideanFilters.GroupByY<int>())
                        .Propagate(Propagators.AllDistinct<int>())
                        .Build(),
                }
            };

            var numericalDomain = new NumericalDomain<int>(new List<Interval<int>>() { new Interval<int>(1, 9) });
            machine.GenerateField(numericalDomain, (9, 9, 1));

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

        private static void SeedField(QMachine<int> machine)
        {
            var grid = EuclideanStateLayer<int>.With(machine.State);

            ConstraintLayer<int>.Collapse(grid[(0, 0, 0)]!, 9, true);
            ConstraintLayer<int>.Collapse(grid[(1, 0, 0)]!, 5, true);
            ConstraintLayer<int>.Collapse(grid[(3, 0, 0)]!, 2, true);
            ConstraintLayer<int>.Collapse(grid[(6, 0, 0)]!, 7, true);

            ConstraintLayer<int>.Collapse(grid[(4, 1, 0)]!, 6, true);
            ConstraintLayer<int>.Collapse(grid[(5, 1, 0)]!, 5, true);

            ConstraintLayer<int>.Collapse(grid[(1, 2, 0)]!, 6, true);
            ConstraintLayer<int>.Collapse(grid[(5, 2, 0)]!, 9, true);
            ConstraintLayer<int>.Collapse(grid[(6, 2, 0)]!, 2, true);

            ConstraintLayer<int>.Collapse(grid[(3, 3, 0)]!, 4, true);
            ConstraintLayer<int>.Collapse(grid[(5, 3, 0)]!, 7, true);
            ConstraintLayer<int>.Collapse(grid[(7, 3, 0)]!, 6, true);
            ConstraintLayer<int>.Collapse(grid[(8, 3, 0)]!, 3, true);

            ConstraintLayer<int>.Collapse(grid[(0, 4, 0)]!, 2, true);
            ConstraintLayer<int>.Collapse(grid[(7, 4, 0)]!, 7, true);

            ConstraintLayer<int>.Collapse(grid[(2, 5, 0)]!, 3, true);

            ConstraintLayer<int>.Collapse(grid[(0, 6, 0)]!, 7, true);
            ConstraintLayer<int>.Collapse(grid[(1, 6, 0)]!, 3, true);
            ConstraintLayer<int>.Collapse(grid[(3, 6, 0)]!, 5, true);
            ConstraintLayer<int>.Collapse(grid[(8, 6, 0)]!, 1, true);

            ConstraintLayer<int>.Collapse(grid[(0, 7, 0)]!, 8, true);
            ConstraintLayer<int>.Collapse(grid[(5, 7, 0)]!, 6, true);

            ConstraintLayer<int>.Collapse(grid[(0, 8, 0)]!, 1, true);
            ConstraintLayer<int>.Collapse(grid[(2, 8, 0)]!, 6, true);
            ConstraintLayer<int>.Collapse(grid[(4, 8, 0)]!, 4, true);
            ConstraintLayer<int>.Collapse(grid[(8, 8, 0)]!, 8, true);
        }
    }
}
