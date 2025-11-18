using System;
using System.Linq;
using qon;
using qon.Layers.VariableLayers;
using qon.Machines;
using qon.Variables;

namespace Examples
{
    internal static class SudokuPrinter
    {
        public static void Print<T>(MachineState<T> state, int size)
        {
            string result = "";
            int blockSize = (int)Math.Sqrt(size);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    var variable = state[v =>
                        v.LayerManager.With<EuclideanLayer<T>>() is var layer
                        && layer.X == x
                        && layer.Y == y].Result.FirstOrDefault();

                    var value = variable?.State != ValueState.Uncertain
                        ? variable?.Value.Value.ToString()
                        : "_";

                    result += $"{value} ";

                    if ((x + 1) % blockSize == 0)
                    {
                        result += " ";
                    }
                }

                result += "\n";

                if ((y + 1) % blockSize == 0)
                {
                    result += "\n";
                }
            }

            Console.Write(result);
        }
    }
}
