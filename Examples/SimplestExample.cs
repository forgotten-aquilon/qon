using System;
using System.Collections.Generic;
using qon;
using qon.Functions.Filters;
using qon.Functions.Propagators;
using qon.Layers.StateLayers;
using qon.Machines;
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

            var machine = QSL.Machine<char>()
                .WithConstraintLayer(new()
                {
                    GeneralConstraints = new()
                    {
                        QSL.CreateConstraint<char>()
                            .Select(QSL.Filters.All<char>())
                            .Propagate(QSL.Propagators.AllDistinct<char>())
                            .Build()
                    }
                })
                .GenerateField(domain, 10);

            foreach (var state in machine.States)
            {
                Console.WriteLine($"{state}: {machine.Status}");
            }
        }
    }
}
