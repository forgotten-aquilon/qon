using qon.Functions.Filters;
using qon.Functions.Mutations;
using qon.Functions.QSL;
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
            var machine = new QMachine<char>(new()
            {
                SolverInjection = DefaultSolver<char>.InjectWith(new DefaultSolver<char>.SolverParameter
                {
                    BackTrackingEnabled = false,
                })
            });

            machine.GenerateField(null, (Settings.GridSize, Settings.GridSize, 1), Optional<char>.Of(Pixel.WhitePixel));

            var center = Settings.GridSize / 2;

            if (EuclideanStateLayer<char>.With(machine.State)[(center, center, 0)] is { } var)
            {
                var.Value = Optional<char>.Of(Pixel.BlackPixel);
            }

            if (EuclideanStateLayer<char>.With(machine.State)[(center+1, center, 0)] is { } var1)
            {
                var1.Value = Optional<char>.Of(Pixel.BlackPixel);
            }

            if (EuclideanStateLayer<char>.With(machine.State)[(center-1, center, 0)] is { } var2)
            {
                var2.Value = Optional<char>.Of(Pixel.BlackPixel);
            }

            if (EuclideanStateLayer<char>.With(machine.State)[(center, center+1, 0)] is { } var3)
            {
                var3.Value = Optional<char>.Of(Pixel.BlackPixel);
            }

            MutationLayer<char>.GetOrCreate(machine.State).Parameter = new MutationLayerParameter<char>
            {
                MutationFunction = QSL.CreateMutation<char>()
                    .Sampling(1)
                    .AddMutation(QSL.Mutation<char>()
                        .Frequency(0.99)
                        .When(Filters.EqualsToValue(Pixel.WhitePixel) & Filters.MooreFilter<char>(neighbors => neighbors.Count(Filters.EqualsToValue(Pixel.BlackPixel).ApplyTo) == 3))
                        .Into(Mutations<char>.ToValue(Pixel.BlackPixel))
                        .Build())
                    .AddMutation(QSL.Mutation<char>()
                        .Frequency(0.99)
                        .When(Filters.EqualsToValue(Pixel.BlackPixel) & Filters.MooreFilter<char>(neighbors => neighbors.Count(Filters.EqualsToValue(Pixel.BlackPixel).ApplyTo) > 3))
                        .Into(Mutations<char>.ToValue(Pixel.WhitePixel))
                        .Build())
                    .AddMutation(QSL.Mutation<char>()
                        .Frequency(0.99)
                        .When(Filters.EqualsToValue(Pixel.BlackPixel) & Filters.MooreFilter<char>(neighbors => neighbors.Count(Filters.EqualsToValue(Pixel.BlackPixel).ApplyTo) < 2))
                        .Into(Mutations<char>.ToValue(Pixel.WhitePixel))
                        .Build())
                    .AddMutation(QSL.Mutation<char>()
                        .Frequency(0.1)
                        .When(Filters.EqualsToValue(Pixel.WhitePixel))
                        .WhenField(f => f.Count(Filters.EqualsToValue(Pixel.BlackPixel).ApplyTo) < 10)
                        .Into(Mutations<char>.ToValue(Pixel.BlackPixel))
                        .Build())
                    .AddMutation(QSL.Mutation<char>()
                        .Frequency(0.001)
                        .When(Filters.EqualsToValue(Pixel.WhitePixel))
                        .Into(Mutations<char>.ToValue(Pixel.BlackPixel))
                        .Build())
                    .AddMutation(QSL.Mutation<char>()
                        .Frequency(0.001)
                        .When(Filters.EqualsToValue(Pixel.BlackPixel))
                        .Into(Mutations<char>.ToValue(Pixel.WhitePixel))
                        .Build())
                    .Build()
                ,
                Fitness = _ => random.Next()
            };

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
