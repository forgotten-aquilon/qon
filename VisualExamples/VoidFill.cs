using qon;
using qon.Functions.Filters;
using qon.Functions.Mutations;
using qon.Functions.Searchers.Anchors;
using qon.Helpers;
using qon.Layers.StateLayers;
using qon.Layers.VariableLayers;
using qon.Machines;
using qon.Solvers;
using qon.Variables.Domains;
using Raylib_cs;
using static Examples.Visual.VisualHelper;
using Color = Raylib_cs.Color;

namespace Examples.Visual
{
    internal static class VoidFill
    {

        public static void Run()
        {
            var random = new Random(104);

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

                    DrawField(machine.State, Settings.GridSize, Settings.GridSize);

                    var statusText = simulationFinished ? "Finished" : "Running";
                    Raylib.DrawText($"{statusText} · Iteration {solver.StepCounter}", 10, Settings.CanvasSize + 8, 20, Color.Black);

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
            var machine = QSL.Machine<char>(new()
            {
                Random = random,
                SolverInit = QSL.DefaultSolver<char>(new()
                {
                    BackTrackingEnabled = true,
                    BackTrackingStrategy = QSL.Decimation
                })
            });

            machine.GenerateField(null, (Settings.GridSize, Settings.GridSize, 1), Optional<char>.Of(Pixel.BlackPixel));

            var center = Settings.GridSize / 2;
            var centerVariable = EuclideanStateLayer<char>.With(machine.State)[(Settings.GridSize-1, Settings.GridSize-1, 0)];

            if (centerVariable is not null)
            {
                centerVariable.Value = Optional<char>.Of(Pixel.RedPixel);
            }

            var endVariable = EuclideanStateLayer<char>.With(machine.State)[(0, 0, 0)];

            if (endVariable is not null)
            {
                endVariable.Value = Optional<char>.Of(Pixel.GreenPixel);
            }

            MutationLayer<char>.GetOrCreate(machine.State).Parameter = new MutationLayerParameter<char>
            {
                MutationFunction = new FallbackMutation<char>(
                    new EuclideanReplacer<char>(
                        EuclideanReplacer<char>.CreatePatternFrom(Pixel.RedPixel, Pixel.BlackPixel, Pixel.GreenPixel),
                        EuclideanReplacer<char>.CreateMutationFrom(Pixel.WhitePixel, Pixel.WhitePixel, Pixel.RedPixel)),
                    new EuclideanReplacer<char>(
                        EuclideanReplacer<char>.CreatePatternFrom(Pixel.RedPixel, Pixel.BlackPixel, Pixel.BlackPixel),
                        EuclideanReplacer<char>.CreateMutationFrom(Pixel.WhitePixel, Pixel.WhitePixel, Pixel.RedPixel))),
                Fitness = field =>
                {
                    var v = field.FirstOrDefault(x => x.Value.CheckValue(Pixel.GreenPixel));

                    if (v is { } greenVariable)
                    {
                        return random.Next(1, 100);
                    }

                    return 0;
                },
                Validation = field =>
                {
                    var v = field.FirstOrDefault(x => x.Value.CheckValue(Pixel.GreenPixel));

                    if (v is {} greenVariable)
                    {
                        return false;
                    }

                    return true;
                }
            };

            return machine;
        }
    }
}
