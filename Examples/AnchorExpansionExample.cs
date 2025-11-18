using System;
using System.Collections.Generic;
using qon;
using qon.Functions.Anchors;
using qon.Functions.Filters;
using qon.Functions.Mutations;
using qon.Functions.Replacers;
using qon.Helpers;
using qon.Layers.StateLayers;
using qon.Machines;
using qon.Solvers;
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
            var machine = new QMachine<char>(
                new QMachineParameter<char> { Random = random });

            machine.GenerateField(new DiscreteDomain<char>('@', '.'), (GridSize, GridSize, 1), Optional<char>.Of('.'));

            var center = GridSize / 2;
            var centerVariable = EuclideanStateLayer<char>.With(machine.State)[(center, center, 0)];
            if (centerVariable is not null)
            {
                centerVariable.Value = Optional<char>.Of('@');
            }

            var anchors = new List<IAnchor<char>>
            {
                Anchors.VNA(Filters.EqualsToValue('@')),
                Anchors.VNA(Filters.EqualsToValue('.'))
            };

            var mutation = new DefaultMutation<char>(
                new BijectiveReplacer<char>(
                    new AnchorManager<char>(anchors),
                    new Mutators.ValueMutator<char>('@', '@')));

            MutationLayer<char>.GetOrCreate(machine.State)._parameter = new MutationLayerParameter<char>
            {
                MutationFunction = mutation.Execute,
                Fitness = _ => random.Next()
            };

            int counter = 0;
            foreach (var state in machine.States)
            {
                Console.Clear();
                Print(state, GridSize, GridSize);
                counter++;
            }
        }

        private static void Print(MachineState<char> state, int width, int height)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var cell = EuclideanStateLayer<char>.With(state)[(x, y, 0)];
                    var value = cell?.Value.TryGetValue(out var v) == true ? v : '.';
                    Console.Write(value);
                }

                Console.Write('\n');
            }
        }
    }
}
