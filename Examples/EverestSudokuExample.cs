using qon;
using qon.Functions.Constraints;
using qon.Functions.Filters;
using qon.Functions.Propagators;
using qon.Machines;
using System;
using System.Threading.Tasks;
using qon.Variables.Domains;

namespace Examples
{
    internal static class EverestSudokuExample
    {
        public static void Run()
        {
            var domain = DomainHelper.SimpleNumericalDomain(1, 9);

            var machine = QSL.Machine<int>(new QMachineParameter<int>
                {
                    Random = new Random(2222)
                })
                .WithConstraintLayer(new()
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
                .GenerateField(domain, (9, 9, 1));

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
            machine.At(0, 0, 0).Collapse(8, true);

            machine.At(2, 1, 0).Collapse(3, true);
            machine.At(3, 1, 0).Collapse(6, true);

            machine.At(1, 2, 0).Collapse(7, true);
            machine.At(4, 2, 0).Collapse(9, true);
            machine.At(6, 2, 0).Collapse(2, true);

            machine.At(1, 3, 0).Collapse(5, true);
            machine.At(5, 3, 0).Collapse(7, true);

            machine.At(4, 4, 0).Collapse(4, true);
            machine.At(5, 4, 0).Collapse(5, true);
            machine.At(6, 4, 0).Collapse(7, true);

            machine.At(3, 5, 0).Collapse(1, true);
            machine.At(7, 5, 0).Collapse(3, true);

            machine.At(2, 6, 0).Collapse(1, true);
            machine.At(7, 6, 0).Collapse(6, true);
            machine.At(8, 6, 0).Collapse(8, true);

            machine.At(2, 7, 0).Collapse(8, true);
            machine.At(3, 7, 0).Collapse(5, true);
            machine.At(7, 7, 0).Collapse(1, true);

            machine.At(1, 8, 0).Collapse(9, true);
            machine.At(6, 8, 0).Collapse(4, true);
        }
    }
}
