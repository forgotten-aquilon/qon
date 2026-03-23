using qon;
using qon.Functions.Constraints;
using qon.Functions.Filters;
using qon.Functions.Propagators;
using qon.Helpers;
using qon.Machines;
using qon.Variables.Domains;
using System;
using System.Collections.Generic;

namespace Examples
{
    internal static class SudokuExample
    {
        public static void Run()
        {
            var numericalDomain = new NumericalDomain<int>(new List<Interval<int>>() { new Interval<int>(1, 9) });

            var machine = QSL.Machine<int>(new QMachineParameter<int>()
                {
                    Random = new Random(1),
                })
                .WithConstraint(new()
                {
                    GeneralConstraints = new()
                    {
                        QSL.CreateConstraint<int>()
                            .GroupBy(QSL.EuclideanFilters.GroupByRectangle<int>(3, 3))
                            .Propagate(QSL.Propagators.AllDistinct<int>())
                            .Build(),
                        QSL.CreateConstraint<int>()
                            .GroupBy(QSL.EuclideanFilters.GroupByX<int>())
                            .Propagate(QSL.Propagators.AllDistinct<int>())
                            .Build(),
                        QSL.CreateConstraint<int>()
                            .GroupBy(QSL.EuclideanFilters.GroupByY<int>())
                            .Propagate(QSL.Propagators.AllDistinct<int>())
                            .Build(),
                    }
                })
                .GenerateField(numericalDomain, (9, 9, 1));

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
            machine.At(0, 0, 0).Collapse(9, true);
            machine.At(1, 0, 0).Collapse(5, true);
            machine.At(3, 0, 0).Collapse(2, true);
            machine.At(6, 0, 0).Collapse(7, true);

            machine.At(4, 1, 0).Collapse(6, true);
            machine.At(5, 1, 0).Collapse(5, true);

            machine.At(1, 2, 0).Collapse(6, true);
            machine.At(5, 2, 0).Collapse(9, true);
            machine.At(6, 2, 0).Collapse(2, true);

            machine.At(3, 3, 0).Collapse(4, true);
            machine.At(5, 3, 0).Collapse(7, true);
            machine.At(7, 3, 0).Collapse(6, true);
            machine.At(8, 3, 0).Collapse(3, true);

            machine.At(0, 4, 0).Collapse(2, true);
            machine.At(7, 4, 0).Collapse(7, true);

            machine.At(2, 5, 0).Collapse(3, true);

            machine.At(0, 6, 0).Collapse(7, true);
            machine.At(1, 6, 0).Collapse(3, true);
            machine.At(3, 6, 0).Collapse(5, true);
            machine.At(8, 6, 0).Collapse(1, true);

            machine.At(0, 7, 0).Collapse(8, true);
            machine.At(5, 7, 0).Collapse(6, true);

            machine.At(0, 8, 0).Collapse(1, true);
            machine.At(2, 8, 0).Collapse(6, true);
            machine.At(4, 8, 0).Collapse(4, true);
            machine.At(8, 8, 0).Collapse(8, true);
        }
    }
}
