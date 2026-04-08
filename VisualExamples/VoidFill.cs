using qon;
using qon.Functions.Filters;
using qon.Functions.Mutations;
using qon.Functions.Searchers.Anchors;
using qon.Helpers;
using qon.Layers.StateLayers;
using qon.Layers.VariableLayers;
using qon.Machines;
using qon.QSL;
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
            var random = new Random();

            var machine = CreateMachine(random);

            Draw(machine,0);
        }

        private static QMachine<char> CreateMachine(Random random)
        {
            var machine = QMachine<char>.Create(new()
            {
                Random = random,
                SolverInit = QSLSolver.DefaultSolver<char>(new()
                {
                    BackTrackingEnabled = true,
                    BackTrackingStrategy = Helpers.Decimation
                })
            })
            .WithMutation(new MutationLayerParameter<char>
            {
                MutationFunction = new FallbackMutation<char>(
                    new CartesianReplacer<char>(
                        CartesianReplacer<char>.CreatePatternFrom(Pixel.RedPixel, Pixel.BlackPixel, Pixel.GreenPixel),
                        CartesianReplacer<char>.CreateMutationFrom(Pixel.WhitePixel, Pixel.WhitePixel, Pixel.RedPixel)),
                    new CartesianReplacer<char>(
                        CartesianReplacer<char>.CreatePatternFrom(Pixel.RedPixel, Pixel.BlackPixel, Pixel.BlackPixel),
                        CartesianReplacer<char>.CreateMutationFrom(Pixel.WhitePixel, Pixel.WhitePixel, Pixel.RedPixel))),
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

                    if (v is { } greenVariable)
                    {
                        return false;
                    }

                    return true;
                }
            })
            .GenerateField((Settings.GridSize, Settings.GridSize, 1), Pixel.BlackPixel); ;

            var center = Settings.GridSize / 2;
            var centerVariable = CartesianStateLayer<char>.On(machine.State)[(Settings.GridSize-1, Settings.GridSize-1, 0)];

            if (centerVariable is not null)
            {
                centerVariable.Value = Pixel.RedPixel;
            }

            var endVariable = CartesianStateLayer<char>.On(machine.State)[(0, 0, 0)];

            if (endVariable is not null)
            {
                endVariable.Value = Pixel.GreenPixel;
            }

            return machine;
        }
    }
}
