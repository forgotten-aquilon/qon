using qon;
using qon.Functions.Filters;
using qon.Functions.Mutations;
using qon.Functions.Searchers.Anchors;
using qon.Helpers;
using qon.Layers.StateLayers;
using qon.Machines;
using qon.Solvers;
using qon.Variables;
using qon.Variables.Domains;
using Raylib_cs;
using Color = Raylib_cs.Color;
using static Examples.Visual.VisualHelper;

namespace Examples.Visual
{
    public static class GameOfLife
    {
        public static void Run()
        {
            var random = new Random(42);
            var machine = CreateMachine(random);

            using var solver = machine.Solver;
            bool simulationFinished = !solver.MoveNext();

            Raylib.InitWindow(Settings.CanvasSize, Settings.CanvasSize + Settings.InfoPanelHeight, "Anchor Expansion Visual");

            try
            {
                while (!Raylib.WindowShouldClose())
                {
                    Raylib.BeginDrawing();
                    Raylib.ClearBackground(Color.White);

                    DrawField(machine.State);

                    var statusText = simulationFinished ? "Finished" : "Running";
                    Raylib.DrawText($"{statusText} · Iteration {solver.StepCounter}", 10, Settings.CanvasSize + 8, 20, Color.Black);

                    Raylib.EndDrawing();

                    Task.Run(async () =>
                    {
                        await Task.Delay(50);
                    }).GetAwaiter().GetResult();

                    if (simulationFinished)
                    {
                        continue;
                    }

                    var moved = solver.MoveNext();

                    if (!moved)
                    {
                        simulationFinished = true;
                    }
                }
            }
            finally
            {
                Raylib.CloseWindow();
            }
        }

        private static QMachine<char> CreateMachine(Random random)
        {
            var machine = QSL.Machine<char>(new() 
            {
                SolverInit = QSL.DefaultSolver<char>(new()
                {
                    BackTrackingEnabled = true,
                })
            })
            .WithMutation(new MutationLayerParameter<char>
            {
                MutationFunction = QSL.CreateMutation<char>()
                .Sampling(1)
                .AddMutation(QSL.Mutation<char>()
                    .Frequency(0.99)
                    .When(QSL.Filters.EqualsToValue(Pixel.WhitePixel) & QSL.Filters.MooreFilter<char>(neighbors => neighbors.Count(QSL.Filters.EqualsToValue(Pixel.BlackPixel).ApplyTo) == 3))
                    .Into(QSL.Mutations<char>.ToValue(Pixel.BlackPixel))
                    .Build())
                .AddMutation(QSL.Mutation<char>()
                    .Frequency(0.99)
                    .When(QSL.Filters.EqualsToValue(Pixel.BlackPixel) & QSL.Filters.MooreFilter<char>(neighbors => neighbors.Count(QSL.Filters.EqualsToValue(Pixel.BlackPixel).ApplyTo) > 3))
                    .Into(QSL.Mutations<char>.ToValue(Pixel.WhitePixel))
                    .Build())
                .AddMutation(QSL.Mutation<char>()
                    .Frequency(0.99)
                    .When(QSL.Filters.EqualsToValue(Pixel.BlackPixel) & QSL.Filters.MooreFilter<char>(neighbors => neighbors.Count(QSL.Filters.EqualsToValue(Pixel.BlackPixel).ApplyTo) < 2))
                    .Into(QSL.Mutations<char>.ToValue(Pixel.WhitePixel))
                    .Build())
                .AddMutation(QSL.Mutation<char>()
                    .Frequency(0.1)
                    .When(QSL.Filters.EqualsToValue(Pixel.WhitePixel))
                    .WhenField(f => f.Count(QSL.Filters.EqualsToValue(Pixel.BlackPixel).ApplyTo) < 10)
                    .Into(QSL.Mutations<char>.ToValue(Pixel.BlackPixel))
                    .Build())
                .AddMutation(QSL.Mutation<char>()
                    .Frequency(0.001)
                    .When(QSL.Filters.EqualsToValue(Pixel.WhitePixel))
                    .Into(QSL.Mutations<char>.ToValue(Pixel.BlackPixel))
                    .Build())
                .AddMutation(QSL.Mutation<char>()
                    .Frequency(0.001)
                    .When(QSL.Filters.EqualsToValue(Pixel.BlackPixel))
                    .Into(QSL.Mutations<char>.ToValue(Pixel.WhitePixel))
                    .Build())
                .Build(),
                Fitness = _ => random.Next()
            })
            .GenerateField(null, (Settings.GridSize, Settings.GridSize, 1), Optional<char>.Of(Pixel.WhitePixel));

            var center = Settings.GridSize / 2;

            machine.At(center, center, 0).Value = Pixel.BlackPixel;
            machine.At(center + 1, center, 0).Value = Pixel.BlackPixel;
            machine.At(center - 1, center, 0).Value = Pixel.BlackPixel;
            machine.At(center, center + 1, 0).Value = Pixel.BlackPixel;

            return machine;
        }

        private static void DrawField(MachineState<char> state)
        {
            var layer = EuclideanStateLayer<char>.With(state);

            for (int y = 0; y < Settings.GridSize; y++)
            {
                for (int x = 0; x < Settings.GridSize; x++)
                {
                    var cell = layer[(x, y, 0)];
                    var pixelValue = Pixel.WhitePixel;
                    if (cell?.Value.TryGetValue(out var resolvedValue) == true)
                    {
                        pixelValue = resolvedValue;
                    }

                    var color = ResolveColor(pixelValue);

                    Raylib.DrawRectangle(x * Settings.PixelSize, y * Settings.PixelSize, Settings.PixelSize, Settings.PixelSize, color);
                }
            }
        }
    }
}
