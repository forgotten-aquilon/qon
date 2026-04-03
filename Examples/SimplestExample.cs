using System;
using System.Collections.Generic;
using qon;
using qon.Functions.Filters;
using qon.Functions.Propagators;
using qon.Layers.StateLayers;
using qon.Machines;
using qon.QSL;
using qon.Solvers;
using qon.Variables.Domains;

namespace Examples
{
    internal static class SimplestExample
    {
        public static void Run()
        {
            var domain = DomainHelper.SymbolicalDomain(
                new DomainHelper.CharDomainOptions()
                    .WithAlphabet('a', 'j'));

            var machine = QMachine<char>.Create(new QMachineParameter<char>(){Random = new Random(1)})
                .WithConstraintLayer(new()
                {
                    GeneralConstraints = new()
                    {
                        Constraints.CreateConstraint<char>()
                            .Select(Filters.All<char>())
                            .Propagate(Propagators.AllDistinct<char>())
                            .Build()
                    }
                })
                .GenerateField(domain, 10);

            foreach (var obj in machine.State.Field)
            {
                Console.WriteLine($"{obj.Id}");
            }

            foreach (var state in machine.States)
            {
                Console.WriteLine($"{state}: {machine.Status}");
            }
        }
    }
}
