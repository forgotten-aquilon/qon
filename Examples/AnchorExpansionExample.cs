using System;
using System.Collections.Generic;
using qon;
using qon.Functions.Filters;
using qon.Functions.Mutations;
using qon.Functions.Searchers.Anchors;
using qon.Helpers;
using qon.Layers.StateLayers;
using qon.Machines;
using qon.QSL;
using qon.Variables;
using qon.Variables.Domains;

namespace Examples
{
    internal static class AnchorExpansionExample
    {
        private const int GridSize = 20;

        public static void Run()
        {
            var random = new Random(42);
            var machine = QMachine<char>.Create(new QMachineParameter<char> { Random = random })
                .WithMutation(new MutationLayerParameter<char>
                {
                    MutationFunction = CreateMutation(),
                    Fitness = _ => random.Next()
                })
                .GenerateField((GridSize, GridSize, 1), new DiscreteDomain<char>('@', '.'), '.');

            var center = GridSize / 2;
            machine.At(center, center, 0).Value = '@';

            int counter = 0;
            foreach (var state in machine.States)
            {
                Console.Clear();
                Print(state, GridSize, GridSize);
                counter++;
            }
        }

        private static MutationFunction<char> CreateMutation()
        {
            var anchors = new List<IAnchor<char>>
            {
                Anchors.VNA(Filters.EqualsToValue('@')),
                Anchors.VNA(Filters.EqualsToValue('.'))
            };

            return new BijectiveReplacer<char>(
                new AnchorManager<char>(anchors),
                new Mutators.ValueMutator<char>('@', '@'));
        }

        private static void Print(MachineState<char> state, int width, int height)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var cell = state.Machine.At(x, y, 0);
                    var value = cell.Value.TryGetValue(out var v) ? v : '.';
                    Console.Write(value);
                }

                Console.Write('\n');
            }
        }
    }
}
