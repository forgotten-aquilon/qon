using qon;
using qon.Functions.Filters;
using qon.Functions.Mutations;
using qon.Helpers;
using qon.Layers.StateLayers;
using qon.Machines;
using qon.QSL;
using qon.Solvers;
using qon.Variables;
using Raylib_cs;
using Color = Raylib_cs.Color;
using static Examples.Visual.VisualHelper;

namespace Examples.Visual
{
    internal static class ForestFire
    {
        public static void Run()
        {
            var random = new Random();
            var machine = CreateMachine(random);

            Draw(machine, 20);
        }

        private static QMachine<char> CreateMachine(Random random)
        {
            var machine = QMachine<char>.Create(new()
            {
                Random = random,
                SolverInit = QSLSolver.DefaultSolver<char>(new()
                {
                    BackTrackingEnabled = false,
                })
            })
                .WithMutation(new()
            {
                MutationFunction = Mutations.CreateMutation<char>()
                    .Sampling(1)
                    .AddMutation(Mutations.Mutation<char>()
                        .Frequency(1.0)
                        .When(Filters.EqualsToValue(Pixel.RedPixel))
                        .Into(Mutations.ToValue<char>(Pixel.WhitePixel))
                        .Build())
                    .AddMutation(Mutations.Mutation<char>()
                        .Frequency(1.0)
                        .When(Filters.EqualsToValue(Pixel.GreenPixel) & CartesianFilters.MooreFilter<char>(neighbors =>
                            neighbors.Any(Filters.EqualsToValue(Pixel.RedPixel).ApplyTo)))
                        .Into(Mutations.ToValue<char>(Pixel.RedPixel))
                        .Build())
                    .AddMutation(Mutations.Mutation<char>()
                        .Frequency(0.0006)
                        .When(Filters.EqualsToValue(Pixel.GreenPixel))
                        .WhenField(f => f.Count(Filters.EqualsToValue(Pixel.RedPixel).ApplyTo) == 0)
                        .Into(Mutations.ToValue<char>(Pixel.RedPixel))
                        .Build())
                    .AddMutation(Mutations.Mutation<char>()
                        .Frequency(0.02)
                        .When(Filters.EqualsToValue(Pixel.WhitePixel))
                        .Into(Mutations.ToValue<char>(Pixel.GreenPixel))
                        .Build())
                    .Build(),
                Fitness = _ => random.Next(),
            })
                .GenerateField((Settings.GridSize, Settings.GridSize, 1), Pixel.WhitePixel);

            SeedInitialForest(machine.State, random);

            return machine;
        }

        private static void SeedInitialForest(MachineState<char> state, Random random)
        {
            var layer = CartesianStateLayer<char>.On(state);

            for (int y = 0; y < Settings.GridSize; y++)
            {
                for (int x = 0; x < Settings.GridSize; x++)
                {
                    var cell = layer[(x, y, 0)];

                    if (cell is null)
                    {
                        continue;
                    }

                    var isTree = random.NextDouble() < 0.62;
                    cell.Value = isTree ? Pixel.GreenPixel : Pixel.WhitePixel;
                }
            }

            for (int i = 0; i < 6; i++)
            {
                var x = random.Next(Settings.GridSize);
                var y = random.Next(Settings.GridSize);
                var spark = layer[(x, y, 0)];

                if (spark is not null)
                {
                    spark.Value = Pixel.RedPixel;
                }
            }
        }
    }
}

