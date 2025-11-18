using System;
using System.Linq;
using qon;
using qon.Layers.VariableLayers;
using qon.Machines;
using qon.Variables;

namespace Examples
{
    internal static class GridPrinter
    {
        public static void Print<T>(MachineState<T> state, int size, bool correctionIndent = false)
        {
            string result = "";
            for (int y = size; y > 0; y--)
            {
                for (int x = 0; x < size; x++)
                {
                    var variable = state[v =>
                        v.Layers.With<EuclideanLayer<T>>() is var layer
                        && layer.X == x
                        && layer.Y == y].Result.FirstOrDefault();

                    var value = variable?.State != ValueState.Uncertain
                        ? variable?.Value.Value.ToString()
                        : "@";

                    result += $"{value}{(correctionIndent ? '\u2009' : "")}";
                }

                result += "\n";
            }

            Console.Write(result);
        }
    }
}
