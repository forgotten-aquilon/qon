using System;
using qon;
using qon.Functions.QSL;
using qon.Functions.Filters;
using qon.Functions.Propagators;
using qon.Machines;
using qon.Solvers;
using qon.Variables.Domains;
using qon.Layers.StateLayers;

namespace Examples
{
    internal static class NumberExample
    {
        public static void Run()
        {
            var machine = new QMachine<int>(new QMachineParameter<int>());

            ConstraintLayer<int>.GetOrCreate(machine.State).Constraints = new()
            {
                GeneralConstraints = new()
                {
                    QSL.Constraint<int>()
                        .Select(Filters.All<int>())
                        .Propagate(Propagators.AllDistinct<int>())
                        .Build()
                }
            };

            machine.GenerateField(new NumericalDomain<int>(), new[] { "V1", "V2", "V3", "V4" });

            foreach (var state in machine.States)
            {
                Console.WriteLine(state);
            }
        }
    }
}
