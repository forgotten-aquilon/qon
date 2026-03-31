using System;
using System.Linq;
using qon;
using qon.Functions.Filters;
using qon.Functions.Mutations;
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
            var target = "ME THINKS IT IS LIKE A WEASEL";

            var machine = QSL.Machine<char>()
                .WithMutation(new MutationLayerParameter<char>
                {
                    MutationFunction = QSL.CreateMutation<char>()
                        .Sampling(100)
                        .AddMutation(QSL.Mutation<char>()
                            .Frequency(0.1)
                            .When(QSL.Filters.All<char>())
                            .Into(QSL.Mutations<char>.RandomFromDomain)
                            .Build())
                        .Build(),
                    Fitness = (field) => Score(field, target)
                })
                .GenerateField((target.Length, 1, 1), 'A');

            foreach (var state in machine.States)
            {
                Console.WriteLine(FormatState(state));
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
