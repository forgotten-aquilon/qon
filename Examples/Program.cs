#pragma warning disable

using System;

namespace Examples
{
    internal static class Program
    {
        private const int DefaultExampleNumber = 7;
        private const int DefaultRotationSize = 22;

        private static int Main(string[] args)
        {
            int exampleNumber =
                args.Length > 0 && int.TryParse(args[0], out int parsedExampleNumber)
                    ? parsedExampleNumber
                    : DefaultExampleNumber;

            switch (10)
            {
                case 1:
                    NumberExample.Run();
                    return 0;
                case 2:
                    BindingExample.Run();
                    return 0;
                case 3:
                    SimplestExample.Run();
                    return 0;
                case 4:
                    SimpleSudokuExample.Run();
                    return 0;
                case 5:
                    SudokuExample.Run();
                    return 0;
                case 6:
                    EverestSudokuExample.Run();
                    return 0;
                case 7:
                    EightQueensExample.Run();
                    return 0;
                case 8:
                    RotationExample.Run(DefaultRotationSize);
                    return 0;
                case 9:
                    MazeExample.Run();
                    return 0;
                case 10:
                    WeaselExample.Run();
                    return 0;
                case 11:
                    AnchorExpansionExample.Run();
                    return 0;
                case 12:
                    SendMoreMoneyExample.Run();
                    return 0;
                default:
                    Console.WriteLine($"Unknown example number '{exampleNumber}'.");
                    return 1;
            }
        }
    }
}
