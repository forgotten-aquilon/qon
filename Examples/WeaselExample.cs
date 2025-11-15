using System;
using System.Linq;
using qon;
using qon.Functions.Filters;
using qon.Functions.Mutations;
using qon.Functions.QSL;
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
            //TODO: Add domain prefabs
            var alphabet = Enumerable.Range('A', 26).Select(n => (char)n).ToList();
            alphabet.Add(' ');
            DiscreteDomain<char> domain = new DiscreteDomain<char>(alphabet);

            var field = "FGUQJPIPMOFUSPW MRHJQMNLF GZP"
                .Select(c => QVariable<char>.New(c, ValueState.Defined))
                .ToArray();

            foreach (var variable in field)
            {
                DomainLayer<char>.GetOrCreate(variable).Domain = domain;
            }

            QMachineParameter<char> parameter = new QMachineParameter<char>
            {
                Field = field,
                Random = new Random(),
            };

            QMachine<char> machine = new QMachine<char>(parameter);

            var target = "ME THINKS IT IS LIKE A WEASEL";

            MutationLayer<char>.GetOrCreate(machine.State)._parameter = new MutationLayerParameter<char>
            {
                MutationFunction = QSL.Mutation<char>()
                    .When(Filters.All<char>())
                    .Sampling(100)
                    .Frequency(0.1)
                    .Into(Mutations<char>.RandomFromDomain)
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
