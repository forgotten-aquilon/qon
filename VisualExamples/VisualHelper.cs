using qon.Layers.StateLayers;
using qon.Machines;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examples.Visual
{
    public static class VisualHelper
    {
        public static class Settings
        {
            public const int GridSize = 41;
            public const int PixelSize = 30;
            public const int CanvasSize = GridSize * PixelSize;
            public const int InfoPanelHeight = 40;
        }

        public static class Pixel
        {
            public const char BlackPixel = '@';
            public const char WhitePixel = 'W';
            public const char GreyPixel = '_';
            public const char BluePixel = 'B';
            public const char CyanPixel = 'C';
            public const char YellowPixel = 'Y';
            public const char RedPixel = '*';
            public const char GreenPixel = 'G';
            public const char DarkGreenPixel = 'F';
        }

        public static Raylib_cs.Color ResolveColor(char value)
        {
            return value switch
            {
                Pixel.BlackPixel => Raylib_cs.Color.Black,
                Pixel.WhitePixel => Raylib_cs.Color.White,
                Pixel.GreyPixel => Raylib_cs.Color.Gray,
                Pixel.BluePixel => Raylib_cs.Color.Blue,
                Pixel.CyanPixel => new Raylib_cs.Color(88, 196, 221, 255),
                Pixel.YellowPixel => new Raylib_cs.Color(232, 211, 125, 255),
                Pixel.RedPixel => Raylib_cs.Color.Red,
                Pixel.GreenPixel => Raylib_cs.Color.Green,
                Pixel.DarkGreenPixel => new Raylib_cs.Color(37, 112, 74, 255),
                _ => Raylib_cs.Color.Magenta
            };
        }

        private static void DrawField(MachineState<char> state)
        {
            var layer = CartesianStateLayer<char>.On(state);

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

        public static void Draw(QMachine<char> machine, int delay = 0)
        {
            using var solver = machine.Solver;
            bool simulationFinished = !solver.MoveNext();

            Raylib.InitWindow(Settings.CanvasSize, Settings.CanvasSize + Settings.InfoPanelHeight, "Forest Fire Visual");

            try
            {
                while (!Raylib.WindowShouldClose())
                {
                    Raylib.BeginDrawing();
                    Raylib.ClearBackground(Color.White);

                    DrawField(machine.State);

                    var statusText = simulationFinished ? "Finished" : "Running";
                    Raylib.DrawText($"{statusText} | Iteration {solver.StepCounter}", 10, Settings.CanvasSize + 8, 20, Color.Black);

                    Raylib.EndDrawing();

                    Task.Run(async () =>
                    {
                        await Task.Delay(delay);
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
    }
}

