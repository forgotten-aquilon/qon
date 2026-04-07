#pragma warning disable

using System;

namespace Examples.Visual
{
    internal static class Program
    {
        private const int DefaultExampleNumber = 4;

        private static int Main(string[] args)
        {
            int exampleNumber =
                args.Length > 0 && int.TryParse(args[0], out int parsedExampleNumber)
                    ? parsedExampleNumber
                    : DefaultExampleNumber;

            switch (5)
            {
                case 1:
                    ConstraintFortressMap.Run();
                    return 0;
                case 2:
                    ConstraintGameMap.Run();
                    return 0;
                case 3:
                    VoidFill.Run();
                    return 0;
                case 4:
                    GameOfLife.Run();
                    return 0;
                case 5:
                    ForestFire.Run();
                    return 0;
                default:
                    Console.WriteLine($"Unknown example number '{exampleNumber}'.");
                    return 1;
            }
        }
    }
}
