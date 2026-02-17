using qon.Functions.Filters;
using qon.Functions.Mutations;
using qon.Functions.Replacers;
using qon.Functions.Searchers.Anchors;
using qon.Helpers;
using qon.Layers.StateLayers;
using qon.Machines;
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
            var random = new Random();
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
            var machine = new QMachine<char>(new ());

            machine.GenerateField(new DiscreteDomain<char>(Pixel.BlackPixel, Pixel.WhitePixel, Pixel.RedPixel, Pixel.GreenPixel), (Settings.GridSize, Settings.GridSize, 1), Optional<char>.Of(Pixel.BlackPixel));

            var center = Settings.GridSize / 2;
            var centerVariable = EuclideanStateLayer<char>.With(machine.State)[(center, center, 0)];

            if (centerVariable is not null)
            {
                centerVariable.Value = Optional<char>.Of(Pixel.RedPixel);
            }

            var endVariable = EuclideanStateLayer<char>.With(machine.State)[(0, 0, 0)];

            if (endVariable is not null)
            {
                endVariable.Value = Optional<char>.Of(Pixel.GreenPixel);
            }

            MutationLayer<char>.GetOrCreate(machine.State)._parameter = new MutationLayerParameter<char>
            {
                MutationFunction = new EuclideanReplacer<char>(
                    EuclideanReplacer<char>.CreatePatternFrom(Pixel.RedPixel, Pixel.BlackPixel, Pixel.BlackPixel), 
                    EuclideanReplacer<char>.CreateMutationFrom(Pixel.WhitePixel, Pixel.WhitePixel, Pixel.RedPixel)),
                Fitness = _ => random.Next(1, 100),
                Validation = field =>
                {
                    var v = field.FirstOrDefault(x => x.Value.CheckValue(Pixel.GreenPixel));

                    if (v is {} greenVariable)
                    {
                        return Filters.MooreFilter<char>(n => n.Count(x => x.Value.CheckValue(Pixel.RedPixel)) > 0)
                            .ApplyTo(greenVariable);
                    }

                    return true;
                }
            };

            return machine;
        }
    }
}
