using qon;
using qon.Functions.Constraints;
using qon.Functions.Filters;
using qon.Functions.Propagators;
using qon.Helpers;
using qon.Machines;
using qon.Variables.Domains;
using System;
using System.Collections.Generic;
using qon.QSL;

namespace Examples
{
    internal static class SimpleSudokuExample
    {
        public static void Run()
        {
            var numericalDomain = new NumericalDomain<int>(new List<Interval<int>>() { new Interval<int>(1, 4) });
            var machine = QMachine<int>.Create(new QMachineParameter<int>() { Random = new Random() })
                .WithConstraintLayer(new()
                {
                    GeneralConstraints = new()
                    {
                        Constraints.CreateConstraint<int>()
                            .GroupBy(CartesianFilters.GroupByRectangle<int>(2, 2))
                            .Propagate(Propagators.AllDistinct<int>())
                            .Build(),
                        Constraints.CreateConstraint<int>()
                            .GroupBy(CartesianFilters.GroupByX<int>())
                            .Propagate(Propagators.AllDistinct<int>())
                            .Build(),
                        Constraints.CreateConstraint<int>()
                            .GroupBy(CartesianFilters.GroupByY<int>())
                            .Propagate(Propagators.AllDistinct<int>())
                            .Build(),
                    }
                })
                .GenerateField((4, 4, 1), numericalDomain);

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
