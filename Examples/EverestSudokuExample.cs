using qon;
using qon.Functions.Constraints;
using qon.Functions.Filters;
using qon.Functions.Propagators;
using qon.Machines;
using System;
using System.Threading.Tasks;
using qon.QSL;
using qon.Variables.Domains;

namespace Examples
{
    internal static class EverestSudokuExample
    {
        public static void Run()
        {
            var domain = DomainHelper.SimpleNumericalDomain(1, 9);

            var machine = QMachine<int>.Create(new QMachineParameter<int>
                {
                    Random = new Random()
                })
                .WithConstraintLayer(new()
                {
                    GeneralConstraints = new()
                    {
                        Constraints.CreateConstraint<int>()
                            .GroupBy(CartesianFilters.GroupByRectangle<int>(3, 3))
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
                .GenerateField((9, 9, 1), domain);

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

        private static void SeedField(QMachine<int> machine)
        {
            machine.At(0, 0, 0).Value = 8;

            machine.At(2, 1, 0).Value = 3;
            machine.At(3, 1, 0).Value = 6;

            machine.At(1, 2, 0).Value = 7;
            machine.At(4, 2, 0).Value = 9;
            machine.At(6, 2, 0).Value = 2;

            machine.At(1, 3, 0).Value = 5;
            machine.At(5, 3, 0).Value = 7;

            machine.At(4, 4, 0).Value = 4;
            machine.At(5, 4, 0).Value = 5;
            machine.At(6, 4, 0).Value = 7;

            machine.At(3, 5, 0).Value = 1;
            machine.At(7, 5, 0).Value = 3;

            machine.At(2, 6, 0).Value = 1;
            machine.At(7, 6, 0).Value = 6;
            machine.At(8, 6, 0).Value = 8;

            machine.At(2, 7, 0).Value = 8;
            machine.At(3, 7, 0).Value = 5;
            machine.At(7, 7, 0).Value = 1;

            machine.At(1, 8, 0).Value = 9;
            machine.At(6, 8, 0).Value = 4;
        }
    }
}
