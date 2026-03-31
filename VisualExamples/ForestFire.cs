using qon;
using qon.Functions.Filters;
using qon.Functions.Mutations;
using qon.Helpers;
using qon.Layers.StateLayers;
using qon.Machines;
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
            var random = new Random(2026);
            var machine = CreateMachine(random);

            Draw(machine);
        }

        private static QMachine<char> CreateMachine(Random random)
        {
            var machine = QSL.Machine<char>(new()
            {
                Random = random,
                SolverInit = QSL.DefaultSolver<char>(new()
                {
                    BackTrackingEnabled = false,
                })
            });

            machine.GenerateField((Settings.GridSize, Settings.GridSize, 1), Pixel.WhitePixel);

            SeedInitialForest(machine.State, random);

            MutationLayer<char>.GetOrCreate(machine.State).Parameter = new MutationLayerParameter<char>
            {
                MutationFunction = QSL.CreateMutation<char>()
                    .Sampling(1)
                    .AddMutation(QSL.Mutation<char>()
                        .Frequency(1.0)
                        .When(QSL.Filters.EqualsToValue(Pixel.RedPixel))
                        .Into(QSL.Mutations<char>.ToValue(Pixel.WhitePixel))
                        .Build())
                    .AddMutation(QSL.Mutation<char>()
                        .Frequency(1.0)
                        .When(QSL.Filters.EqualsToValue(Pixel.GreenPixel) & QSL.Filters.MooreFilter<char>(neighbors =>
                            neighbors.Any(QSL.Filters.EqualsToValue(Pixel.RedPixel).ApplyTo)))
                        .Into(QSL.Mutations<char>.ToValue(Pixel.RedPixel))
                        .Build())
                    .AddMutation(QSL.Mutation<char>()
                        .Frequency(0.0006)
                        .When(QSL.Filters.EqualsToValue(Pixel.GreenPixel))
                        .WhenField(f => f.Count(QSL.Filters.EqualsToValue(Pixel.RedPixel).ApplyTo) == 0)
                        .Into(QSL.Mutations<char>.ToValue(Pixel.RedPixel))
                        .Build())
                    .AddMutation(QSL.Mutation<char>()
                        .Frequency(0.02)
                        .When(QSL.Filters.EqualsToValue(Pixel.WhitePixel))
                        .Into(QSL.Mutations<char>.ToValue(Pixel.GreenPixel))
                        .Build())
                    .Build(),
                Fitness = _ => random.Next(),
            };

            return machine;
        }

        private static void SeedInitialForest(MachineState<char> state, Random random)
        {
            var layer = EuclideanStateLayer<char>.With(state);

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
                    cell.Value = Optional<char>.Of(isTree ? Pixel.GreenPixel : Pixel.WhitePixel);
                }
            }

            for (int i = 0; i < 6; i++)
            {
                var x = random.Next(Settings.GridSize);
                var y = random.Next(Settings.GridSize);
                var spark = layer[(x, y, 0)];

                if (spark is not null)
                {
                    spark.Value = Optional<char>.Of(Pixel.RedPixel);
                }
            }
        }
    }
}

