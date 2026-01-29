using qon.Functions.Filters;
using qon.Functions.Mutations;
using qon.Functions.QSL;
using qon.Functions.Replacers;
using qon.Functions.Searchers.Anchors;
using qon.Helpers;
using qon.Layers.StateLayers;
using qon.Machines;
using qon.Variables;
using qon.Variables.Domains;
using Raylib_cs;
using Color = Raylib_cs.Color;

namespace Examples.Visual
{
    internal static class GameOfLife
    {
        private const int GridSize = 30;
        private const int PixelSize = 20;
        private const int CanvasSize = GridSize * PixelSize;
        private const int InfoPanelHeight = 40;
        private const char BlackPixel = '@';
        private const char WhitePixel = ' ';

        public static void Run()
        {
            var random = new Random(42);
            var machine = CreateMachine(random);

            using var solver = machine.Solver;
            bool simulationFinished = !solver.MoveNext();

            Raylib.InitWindow(CanvasSize, CanvasSize + InfoPanelHeight, "Anchor Expansion Visual");

            try
            {
                while (!Raylib.WindowShouldClose())
                {
                    Raylib.BeginDrawing();
                    Raylib.ClearBackground(Color.White);

                    DrawField(machine.State);

                    var statusText = simulationFinished ? "Finished" : "Running";
                    Raylib.DrawText($"{statusText} · Iteration {solver.StepCounter}", 10, CanvasSize + 8, 20, Color.Black);

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
            var machine = new QMachine<char>(new ());

            machine.GenerateField(new DiscreteDomain<char>(BlackPixel, WhitePixel), (GridSize, GridSize, 1), Optional<char>.Of(WhitePixel));

            var center = GridSize / 2;

            if (EuclideanStateLayer<char>.With(machine.State)[(center, center, 0)] is { } var)
            {
                var.Value = Optional<char>.Of(BlackPixel);
            }

            if (EuclideanStateLayer<char>.With(machine.State)[(center+1, center, 0)] is { } var1)
            {
                var1.Value = Optional<char>.Of(BlackPixel);
            }

            if (EuclideanStateLayer<char>.With(machine.State)[(center-1, center, 0)] is { } var2)
            {
                var2.Value = Optional<char>.Of(BlackPixel);
            }

            if (EuclideanStateLayer<char>.With(machine.State)[(center, center+1, 0)] is { } var3)
            {
                var3.Value = Optional<char>.Of(BlackPixel);
            }

            MutationLayer<char>.GetOrCreate(machine.State)._parameter = new MutationLayerParameter<char>
            {
                //TODO: When() for field state
                MutationFunction = QSL.CreateMutation<char>()
                    .Sampling(1)
                    .AddMutation(QSL.Mutation<char>()
                        .Frequency(0.99)
                        .When(Filters.EqualsToValue(WhitePixel) & Filters.MooreFilter<char>(neighbors => neighbors.Count(Filters.EqualsToValue(BlackPixel).ApplyTo) == 3))
                        .Into(Mutations<char>.ToValue(BlackPixel))
                        .Build())
                    .AddMutation(QSL.Mutation<char>()
                        .Frequency(0.99)
                        .When(Filters.EqualsToValue(BlackPixel) & Filters.MooreFilter<char>(neighbors => neighbors.Count(Filters.EqualsToValue(BlackPixel).ApplyTo) > 3))
                        .Into(Mutations<char>.ToValue(WhitePixel))
                        .Build())
                    .AddMutation(QSL.Mutation<char>()
                        .Frequency(0.99)
                        .When(Filters.EqualsToValue(BlackPixel) & Filters.MooreFilter<char>(neighbors => neighbors.Count(Filters.EqualsToValue(BlackPixel).ApplyTo) < 2))
                        .Into(Mutations<char>.ToValue(WhitePixel))
                        .Build())
                    .AddMutation(QSL.Mutation<char>()
                        .Frequency(0.1)
                        .When(Filters.EqualsToValue(WhitePixel) & Filters.FieldFilter<char>(f => f.Count(Filters.EqualsToValue(BlackPixel).ApplyTo) < 10))
                        .Into(Mutations<char>.ToValue(BlackPixel))
                        .Build())
                    .AddMutation(QSL.Mutation<char>()
                        .Frequency(0.001)
                        .When(Filters.EqualsToValue(WhitePixel))
                        .Into(Mutations<char>.ToValue(BlackPixel))
                        .Build())
                    .AddMutation(QSL.Mutation<char>()
                        .Frequency(0.001)
                        .When(Filters.EqualsToValue(BlackPixel))
                        .Into(Mutations<char>.ToValue(WhitePixel))
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

            for (int y = 0; y < GridSize; y++)
            {
                for (int x = 0; x < GridSize; x++)
                {
                    var cell = layer[(x, y, 0)];
                    var pixelValue = WhitePixel;
                    if (cell?.Value.TryGetValue(out var resolvedValue) == true)
                    {
                        pixelValue = resolvedValue;
                    }

                    var color = ResolveColor(pixelValue);

                    Raylib.DrawRectangle(x * PixelSize, y * PixelSize, PixelSize, PixelSize, color);
                }
            }
        }

        private static Color ResolveColor(char value)
        {
            return value == BlackPixel ? Color.Black : Color.White;
        }
    }
}
