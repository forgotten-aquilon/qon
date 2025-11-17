using qon;
using qon.Functions.Constraints;
using qon.Functions.QSL;
using qon.Functions.Filters;
using qon.Functions.Propagators;
using qon.Helpers;
using qon.Machines;
using qon.Solvers;
using System;
using System.Collections.Generic;
using qon.Layers.StateLayers;
using qon.Variables.Domains;

namespace Examples
{
    internal static class SimpleSudokuExample
    {
        public static void Run()
        {
            var numericalDomain = new NumericalDomain<int>(new List<Interval<int>>() { new Interval<int>(1, 4) });
            var machine = new QMachine<int>(new QMachineParameter<int>(){Random = new Random(12) });
            ConstraintLayer<int>.GetOrCreate(machine.State).Constraints = new()
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
            };
            machine.GenerateField(numericalDomain, (4, 4, 1));

            foreach (var state in machine.States)
            {
                Console.Clear();
                SudokuPrinter.Print(state, 4);
                Console.WriteLine(machine.Solver.StepCounter);
            }
        }
    }
}
