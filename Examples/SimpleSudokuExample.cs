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
    internal static class SimpleSudokuExample
    {
        public static void Run()
        {
            var numericalDomain = new NumericalDomain<int>(new List<Interval<int>>() { new Interval<int>(1, 4) });
            var machine = QSL.Machine<int>(new QMachineParameter<int>() { Random = new Random() })
                .WithConstraintLayer(new()
                {
                    GeneralConstraints = new()
                    {
                        QSL.CreateConstraint<int>()
                            .GroupBy(QSL.EuclideanFilters.GroupByRectangle<int>(2, 2))
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
                .GenerateField(numericalDomain, (4, 4, 1));

            machine.At(0, 0, 0).Collapse(1, true);

            foreach (var state in machine.States)
            {
                Console.Clear();
                SudokuPrinter.Print(state, 4);
                Console.WriteLine(machine.Solver.StepCounter);
            }
        }
    }
}
