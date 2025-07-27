// See https://aka.ms/new-console-template for more information
#pragma warning disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using qon;
using qon.Domains;
using qon.Rules;

Maze();

void SimpleExample()
{
    var parameter = new QMachineParameter<char>()
    {
        GlobalRules = new()
        {
            new GlobalRule<char>(Aggregators.All<char>(), Filters.AllDistinct<char>())
        },
    };

    var machine = new QMachine<char>(parameter);

    List<char> letters = new() { 'a', 'b', 'c', 'd', 'e', 'f', 'g' };
    machine.GenerateField(new DiscreteDomain<char>(letters), new[] { "V1", "V2", "V3", "V4" });

    foreach (var state in machine.States)
    {
        Console.WriteLine(state);
    }
}

void SimpleSudoku()
{
    List<int> domain = new List<int>() { 1, 2, 3, 4 };

    var p = new QMachineParameter<int>()
    {
        GlobalRules = new List<IGlobalRule<int>>() {
            new GlobalRule<int>(Aggregators.GroupByTag<int>("x"), Filters.AllDistinct<int>()),
            new GlobalRule<int>(Aggregators.GroupByTag<int>("y"), Filters.AllDistinct<int>()),
            new GlobalRule<int>(EuclideanAggregators.GroupByRectangle<int>(2,2), Filters.AllDistinct<int>())
        }
    };

    var w = new WFCMachine<int>(p);
    w.CreateEuclideanSpace((4, 4, 1), new DiscreteDomain<int>(domain));

    int step = 0;

    foreach (var state in w.States)
    {
        Console.Clear();
        PrintSudoku(state, 4);
        Console.WriteLine(step);
        step++;
    }
}

void Sudoku()
{
    List<int> domain = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

    var p = new QMachineParameter<int>()
    {
        GlobalRules = new List<IGlobalRule<int>>() {
            new GlobalRule<int>(Aggregators.GroupByTag<int>("x"), Filters.AllDistinct<int>()),
            new GlobalRule<int>(Aggregators.GroupByTag<int>("y"), Filters.AllDistinct<int>()),
            new GlobalRule<int>(EuclideanAggregators.GroupByRectangle<int>(3,3), Filters.AllDistinct<int>())
        }
    };

    var w = new WFCMachine<int>(p);
    w.CreateEuclideanSpace((9, 9, 1), new DiscreteDomain<int>(domain));

    #region Field init
    w[0, 0].Collapse(9, true);
    w[1, 0].Collapse(5, true);
    w[3, 0].Collapse(2, true);
    w[6, 0].Collapse(7, true);

    w[4, 1].Collapse(6, true);
    w[5, 1].Collapse(5, true);

    w[1, 2].Collapse(6, true);
    w[5, 2].Collapse(9, true);
    w[6, 2].Collapse(2, true);

    w[3, 3].Collapse(4, true);
    w[5, 3].Collapse(7, true);
    w[7, 3].Collapse(6, true);
    w[8, 3].Collapse(3, true);

    w[0, 4].Collapse(2, true);
    w[7, 4].Collapse(7, true);

    w[2, 5].Collapse(3, true);

    w[0, 6].Collapse(7, true);
    w[1, 6].Collapse(3, true);
    w[3, 6].Collapse(5, true);
    w[8, 6].Collapse(1, true);

    w[0, 7].Collapse(8, true);
    w[5, 7].Collapse(6, true);

    w[0, 8].Collapse(1, true);
    w[2, 8].Collapse(6, true);
    w[4, 8].Collapse(4, true);
    w[8, 8].Collapse(8, true);
    #endregion

    int step = 0;

    foreach (var state in w.States)
    {
        Console.Clear();
        PrintSudoku(state, 9);
        Console.WriteLine(step);
        step++;
    }
}

void EverestSudoku()
{
    List<int> domain = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

    var p = new QMachineParameter<int>()
    {
        GlobalRules = new List<IGlobalRule<int>>() {
            new GlobalRule<int>(Aggregators.GroupByTag<int>("x"), Filters.AllDistinct<int>()),
            new GlobalRule<int>(Aggregators.GroupByTag<int>("y"), Filters.AllDistinct<int>()),
            new GlobalRule<int>(EuclideanAggregators.GroupByRectangle<int>(3,3), Filters.AllDistinct<int>())
        },
        Random = new Random()
    };

    var w = new WFCMachine<int>(p);
    w.CreateEuclideanSpace((9, 9, 1), new DiscreteDomain<int>(domain));

    #region Field init  
    w[0, 0].Collapse(8, true);

    w[2, 1].Collapse(3, true);
    w[3, 1].Collapse(6, true);

    w[1, 2].Collapse(7, true);
    w[4, 2].Collapse(9, true);
    w[6, 2].Collapse(2, true);

    w[1, 3].Collapse(5, true);
    w[5, 3].Collapse(7, true);

    w[4, 4].Collapse(4, true);
    w[5, 4].Collapse(5, true);
    w[6, 4].Collapse(7, true);

    w[3, 5].Collapse(1, true);
    w[7, 5].Collapse(3, true);

    w[2, 6].Collapse(1, true);
    w[7, 6].Collapse(6, true);
    w[8, 6].Collapse(8, true);

    w[2, 7].Collapse(8, true);
    w[3, 7].Collapse(5, true);
    w[7, 7].Collapse(1, true);

    w[1, 8].Collapse(9, true);
    w[6, 8].Collapse(4, true);
    #endregion

    int step = 0;

    foreach (var state in w.States)
    {
        Console.Clear();
        PrintSudoku(state, 9);
        Console.WriteLine(step);
        step++;
        Task.Run(async () =>
        {
            await Task.Delay(50);
        }).GetAwaiter().GetResult();
    }
}

void PrintSudoku<T>(MachineState<T> s, int size)
{
    string result = "";
    for (int i = 0; i < size; i++)
    {
        for (int j = 0; j < size; j++)
        {
            var a = s[new List<(string Name, object Value)>
                { ("x", j), ("y", i) }].FirstOrDefault();
            var b = a.State != SuperpositionState.Uncertain ? a.Value.Value.ToString() : "_";

            result += $"{b} ";

            if ((j + 1) % 3 == 0)
            {
                result += $" ";
            }
        }

        result += "\n";

        if ((i + 1) % 3 == 0)
        {
            result += "\n";
        }
    }

    Console.Write(result);
}

void Maze()
{
    List<string> domain = new() { "╬", "║", "═", "╔", "╗", "╚", "╝", "╠", "╣", "╩", "╦", " " };

    List<string> leftConn = new() { "╬", "═", "╔", "╚", "╠", "╩", "╦" };
    List<string> leftWall = new() { "║", "╗", "╝", "╣", " " };

    List<string> rightConn = new() { "╬", "═", "╗", "╝", "╣", "╩", "╦" };
    List<string> rightWall = new() { "║", "╚", "╔", "╠", " " };

    List<string> topConn = new() { "╬", "║", "╗", "╔", "╣", "╠", "╦" };
    List<string> topWall = new() { "═", "╚", "╝", "╩", " " };

    List<string> bottomConn = new() { "╬", "║", "╚", "╝", "╣", "╠", "╩" };
    List<string> bottomWall = new() { "╔", "╗", "═", "╦", " " };

    var p = new QMachineParameter<string>()
    {
        VariableRules = new() {
            new EuclideanRule<string>(new(){Guards.Equals("╬")},
                new EuclideanRuleParameter<string>(){
                    Left = leftConn,
                    Right = rightConn,
                    Front = topConn,
                    Back = bottomConn,
                }),
            new EuclideanRule<string>(new() { Guards.Equals("║") },
                new EuclideanRuleParameter<string>(){
                    Left = leftWall,
                    Right = rightWall,
                    Front = topConn,
                    Back = bottomConn,
                }),
            new EuclideanRule<string>(new() { Guards.Equals("═") },
                new EuclideanRuleParameter<string>(){
                    Left = leftConn,
                    Right = rightConn,
                    Front = topWall,
                    Back = bottomWall,
                }),
            new EuclideanRule<string>(new() { Guards.Equals("╔") },
                new EuclideanRuleParameter<string>(){
                    Left = leftWall,
                    Right = rightConn,
                    Front = topWall,
                    Back = bottomConn,
                }),
            new EuclideanRule<string>(new() { Guards.Equals("╗") },
                new EuclideanRuleParameter<string>(){
                    Left = leftConn,
                    Right = rightWall,
                    Front = topWall,
                    Back = bottomConn,
                }),
            new EuclideanRule<string>(new() { Guards.Equals("╚") },
                new EuclideanRuleParameter<string>(){
                    Left = leftWall,
                    Right = rightConn,
                    Front = topConn,
                    Back = bottomWall,
                }),
            new EuclideanRule<string>(new() { Guards.Equals("╝") },
                new EuclideanRuleParameter<string>(){
                    Left = leftConn,
                    Right = rightWall,
                    Front = topConn,
                    Back = bottomWall,
                }),
            new EuclideanRule<string>(new() { Guards.Equals("╠") },
                new EuclideanRuleParameter<string>(){
                    Left = leftWall,
                    Right = rightConn,
                    Front = topConn,
                    Back = bottomConn,
                }),
            new EuclideanRule<string>(new() { Guards.Equals("╣") },
                new EuclideanRuleParameter<string>(){
                    Left = leftConn,
                    Right = rightWall,
                    Front = topConn,
                    Back = bottomConn,
                }),
            new EuclideanRule<string>(new() { Guards.Equals("╦") },
                new EuclideanRuleParameter<string>(){
                    Left = leftConn,
                    Right = rightConn,
                    Front = topWall,
                    Back = bottomConn,
                }),
            new EuclideanRule<string>(new() { Guards.Equals("╩") },
                new EuclideanRuleParameter<string>(){
                    Left = leftConn,
                    Right = rightConn,
                    Front = topConn,
                    Back = bottomWall,
                }),
            new EuclideanRule<string>(new() { Guards.Equals(" ") },
                new EuclideanRuleParameter<string>(){
                    Left = leftWall,
                    Right = rightWall,
                    Front = topWall,
                    Back = bottomWall,
                }),
        }
    };

    WFCMachine<string> w = new WFCMachine<string>(p);

    w.CreateEuclideanSpace((10, 10, 1), new DiscreteDomain<string>(domain));

    foreach (var variable in w.State.Field)
    {
        variable.Domain.UpdateWeight(" ", 5);
    }

    foreach (var state in w.States)
    {
        Console.Clear();
        Print(state, 10);
    }
}

void Print<T>(MachineState<T> s, int size)
{
    string result = "";
    for (int i = 0; i < size; i++)
    {
        for (int j = 0; j < size; j++)
        {
            var a = s["x", j]["y", i].Result.FirstOrDefault();
            var b = a.State != SuperpositionState.Uncertain ? a.Value.Value.ToString() : "@";

            result += $"{b}";
        }

        result += "\n";
    }

    Console.Write(result);
}

