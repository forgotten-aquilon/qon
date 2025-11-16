using System;
using System.Collections.Generic;
using qon;
using qon.Functions.QSL;
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
            var machine = new QMachine<char>(new QMachineParameter<char>());
            ConstraintLayer<char>.GetOrCreate(machine.State).Constraints = new()
            {
                GeneralConstraints = new()
                {
                    QSL.Constraint<char>()
                        .Select(Filters.All<char>())
                        .Propagate(Propagators.AllDistinct<char>())
                        .Build()
                }
            };

            List<char> letters = new() { 'a', 'b', 'c', 'd', 'e', 'f', 'g' };
            //var domain = DomainHelper.SymbolicalDomain(
            //    new DomainHelper.CharDomainOptions()
            //        .WithAlphabet('a', 'e'));
            var domain = new PrimitiveDomain<char>(new HashSet<char>(letters));
            machine.GenerateField(domain, new[] { "V1", "V2", "V3", "V4" });

            foreach (var state in machine.States)
            {
                Console.WriteLine(state);
                Console.WriteLine(machine.StateType);
            }
        }
    }
}
