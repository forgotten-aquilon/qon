using qon.Functions.Filters;
using qon.Functions.Mutations;
using qon.Functions.Replacers;
using qon.Functions.Searchers.Anchors;
using qon.Helpers;
using qon.Layers.StateLayers;
using qon.Machines;
using qon.Variables.Domains;
using Raylib_cs;
using Color = Raylib_cs.Color;

namespace Examples.Visual
{
    internal static class VoidFill
    {
        private const int GridSize = 30;
        private const int PixelSize = 5;
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
            var centerVariable = EuclideanStateLayer<char>.With(machine.State)[(10, 20, 0)];
            if (centerVariable is not null)
            {
                centerVariable.Value = Optional<char>.Of(BlackPixel);
            }

            var anchors = new List<IAnchor<char>>
            {
                Anchors.VNA(Filters.EqualsToValue(BlackPixel)),
                Anchors.VNA(Filters.EqualsToValue(WhitePixel))
            };

            var mutations = new List<VariableMutation<char>>
            {
                new(v => v.Value = Optional<char>.Of(BlackPixel)),
                new(v => v.Value = Optional<char>.Of(BlackPixel)),
            };

            MutationLayer<char>.GetOrCreate(machine.State)._parameter = new MutationLayerParameter<char>
            {
                MutationFunction = new EuclideanReplacer<char>(
                    EuclideanReplacer<char>.CreatePatternFrom(BlackPixel, WhitePixel), 
                    EuclideanReplacer<char>.CreateMutationFrom(BlackPixel, BlackPixel)),
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
