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
            public const int GridSize = 10;
            public const int PixelSize = 30;
            public const int CanvasSize = GridSize * PixelSize;
            public const int InfoPanelHeight = 40;
        }

        public static class Pixel
        {
            public const char BlackPixel = '@';
            public const char WhitePixel = ' ';
            public const char GreyPixel = '_';
            public const char RedPixel = '*';
            public const char GreenPixel = 'G';
        }

        public static Raylib_cs.Color ResolveColor(char value)
        {
            return value switch
            {
                Pixel.BlackPixel => Raylib_cs.Color.Black,
                Pixel.WhitePixel => Raylib_cs.Color.White,
                Pixel.GreyPixel => Raylib_cs.Color.Gray,
                Pixel.RedPixel => Raylib_cs.Color.Red,
                Pixel.GreenPixel => Raylib_cs.Color.Green,
                _ => Raylib_cs.Color.Magenta
            };
        }



        public static void DrawField(MachineState<char> state, int xSize, int ySize)
        {
            var layer = EuclideanStateLayer<char>.With(state);

            for (int y = 0; y < ySize; y++)
            {
                for (int x = 0; x < xSize; x++)
                {
                    var cell = layer[(x, y, 0)];

                    var color = ResolveColor(cell?.Value.HasValue ?? false ? cell.Value.Value : char.MaxValue);

                    Raylib.DrawRectangle(x * Settings.PixelSize, y * Settings.PixelSize, Settings.PixelSize, Settings.PixelSize, color);
                }
            }
        }
    }
}
