using System;
using System.Linq;
using qon;
using qon.Functions.Filters;
using qon.Functions.Mutations;
using qon.Functions.QSL;
using qon.Helpers;
using qon.Layers.StateLayers;
using qon.Layers.VariableLayers;
using qon.Machines;
using qon.Solvers;
using qon.Variables;
using qon.Variables.Domains;

namespace Examples
{
    internal static class WeaselExample
    {
        public static void Run()
        {
            var domain = DomainHelper.SymbolicalDomain(new DomainHelper.CharDomainOptions()
                    .WithAlphabet('A', 'Z')
                    .WithOtherSymbols(' '));

            QMachine<char> machine = new QMachine<char>(new QMachineParameter<char>());

            machine.GenerateField(domain, (29,1,1), Optional<char>.Of('A'));

            var target = "ME THINKS IT IS LIKE A WEASEL";

            MutationLayer<char>.GetOrCreate(machine.State)._parameter = new MutationLayerParameter<char>
            {
                MutationFunction = QSL.CreateMutation<char>()
                    .Sampling(50)
                    .AddMutation(QSL.Mutation<char>()
                        .Frequency(0.1)
                        .When(Filters.All<char>())
                        .Into(Mutations<char>.RandomFromDomain)
                        .Build())
                    .Build(),
                Fitness = BuildFitness(target)
            };

            foreach (var state in machine.States)
            {
                Console.WriteLine(FormatState(state));
            }

            Func<Field<char>, int> BuildFitness(string template)
            {
                return sample => Score(sample, template);
            }

            int Score(Field<char> first, string second)
            {
                if (first.Count != second.Length)
                {
                    return -1;
                }

                int mismatch = 0;
                    
                for (int i = 0; i < first.Count; i++)
                {
                    if (!first[i].Value.CheckValue(second[i]))
                    {
                        mismatch++;
                    }
                }

                return mismatch;
            }

            string FormatState(MachineState<char> state)
            {
                return new string(state.Field.Select(variable => variable.Value.Value).ToArray());
            }
        }
    }
}
