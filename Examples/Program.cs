// See https://aka.ms/new-console-template for more information
#pragma warning disable

using qon;
using qon.Domains;
using qon.Exceptions;
using qon.Helpers;
using qon.Variables;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using qon.Functions.Propagators;
using qon.Functions.Filters;
using qon.Functions.Constraints;
using qon.Functions.DSL;
using qon.Functions.Operations;
using qon.Variables.Layers;

for (; ; )
{

}

//RotationExample(22);
Maze();

void NumberExample()
{
    var parameter = new QMachineParameter<int>()
    {
        Constraints = new()
        {
            GeneralConstraints = new()
            {
                 QSL.Constraint<int>()
                    .Select(Filters.All<int>())
                    .Propagate(Propagators.AllDistinct<int>())
                    .Build()
            }
        }
    };

    var machine = new QMachine<int>(parameter);

    machine.GenerateField(new NumericalDomain<int>(), new[] { "V1", "V2", "V3", "V4" });

    foreach (var state in machine.States)
    {
        Console.WriteLine(state);
    }
}

void SimplestExample()
{
    var parameter = new QMachineParameter<char>()
    {
        Constraints = new()
        {
            GeneralConstraints = new()
            {
                 QSL.Constraint<char>()
                    .Select(Filters.All<char>())
                    .Propagate(Propagators.AllDistinct<char>())
                    .Build()
            }
        }
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
        Constraints = new()
        {
            GeneralConstraints = new()
            {
                 QSL.Constraint<int>()
                    .GroupBy(EuclideanFilters.GroupByRectangle<int>(2, 2))
                    .Propagate(Propagators.AllDistinct<int>())
                    .Build(),
                 QSL.Constraint<int>()
                    .GroupBy(EuclideanFilters.GroupByX<int>())
                    .Propagate(Propagators.AllDistinct<int>())
                    .Build(),
                 QSL.Constraint<int>()
                    .GroupBy(EuclideanFilters.GroupByY<int>())
                    .Propagate(Propagators.AllDistinct<int>())
                    .Build(),
            }
        },
        Random = new Random(12)
    };
    var nd = new NumericalDomain<int>(new List<Interval<int>>() { new Interval<int>(1, 4) });
    var d = new DiscreteDomain<int>(domain);
    var w = new WFCMachine<int>(p);
    w.CreateEuclideanSpace((4, 4, 1), nd);

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
        Constraints = new()
        {
            GeneralConstraints = new()
            {
                 QSL.Constraint<int>()
                    .GroupBy(EuclideanFilters.GroupByRectangle<int>(2, 2))
                    .Propagate(Propagators.AllDistinct<int>())
                    .Build(),
                 QSL.Constraint<int>()
                    .GroupBy(EuclideanFilters.GroupByX<int>())
                    .Propagate(Propagators.AllDistinct<int>())
                    .Build(),
                 QSL.Constraint<int>()
                    .GroupBy(EuclideanFilters.GroupByY<int>())
                    .Propagate(Propagators.AllDistinct<int>())
                    .Build(),
            }
        }

    };

    var w = new WFCMachine<int>(p);
    var nd = new NumericalDomain<int>(new List<Interval<int>>() { new Interval<int>(1, 9) });
    w.CreateEuclideanSpace((9, 9, 1), nd);

    #region Field init
    SuperpositionLayer<int>.Collapse(w[(0, 0, 0)]!, 9, true);
    SuperpositionLayer<int>.Collapse(w[(1, 0, 0)]!, 5, true);
    SuperpositionLayer<int>.Collapse(w[(3, 0, 0)]!, 2, true);
    SuperpositionLayer<int>.Collapse(w[(6, 0, 0)]!, 7, true);

    SuperpositionLayer<int>.Collapse(w[(4, 1, 0)]!, 6, true);
    SuperpositionLayer<int>.Collapse(w[(5, 1, 0)]!, 5, true);

    SuperpositionLayer<int>.Collapse(w[(1, 2, 0)]!, 6, true);
    SuperpositionLayer<int>.Collapse(w[(5, 2, 0)]!, 9, true);
    SuperpositionLayer<int>.Collapse(w[(6, 2, 0)]!, 2, true);

    SuperpositionLayer<int>.Collapse(w[(3, 3, 0)]!, 4, true);
    SuperpositionLayer<int>.Collapse(w[(5, 3, 0)]!, 7, true);
    SuperpositionLayer<int>.Collapse(w[(7, 3, 0)]!, 6, true);
    SuperpositionLayer<int>.Collapse(w[(8, 3, 0)]!, 3, true);

    SuperpositionLayer<int>.Collapse(w[(0, 4, 0)]!, 2, true);
    SuperpositionLayer<int>.Collapse(w[(7, 4, 0)]!, 7, true);

    SuperpositionLayer<int>.Collapse(w[(2, 5, 0)]!, 3, true);

    SuperpositionLayer<int>.Collapse(w[(0, 6, 0)]!, 7, true);
    SuperpositionLayer<int>.Collapse(w[(1, 6, 0)]!, 3, true);
    SuperpositionLayer<int>.Collapse(w[(3, 6, 0)]!, 5, true);
    SuperpositionLayer<int>.Collapse(w[(8, 6, 0)]!, 1, true);

    SuperpositionLayer<int>.Collapse(w[(0, 7, 0)]!, 8, true);
    SuperpositionLayer<int>.Collapse(w[(5, 7, 0)]!, 6, true);

    SuperpositionLayer<int>.Collapse(w[(0, 8, 0)]!, 1, true);
    SuperpositionLayer<int>.Collapse(w[(2, 8, 0)]!, 6, true);
    SuperpositionLayer<int>.Collapse(w[(4, 8, 0)]!, 4, true);
    SuperpositionLayer<int>.Collapse(w[(8, 8, 0)]!, 8, true);
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
        Constraints = new()
        {
            GeneralConstraints = new()
            {
                 QSL.Constraint<int>()
                    .GroupBy(EuclideanFilters.GroupByRectangle<int>(3, 3))
                    .Propagate(Propagators.AllDistinct<int>())
                    .Build(),
                 QSL.Constraint<int>()
                    .GroupBy(EuclideanFilters.GroupByX<int>())
                    .Propagate(Propagators.AllDistinct<int>())
                    .Build(),
                 QSL.Constraint<int>()
                    .GroupBy(EuclideanFilters.GroupByY<int>())
                    .Propagate(Propagators.AllDistinct<int>())
                    .Build(),
            }
        },
        Random = new Random(2222)
    };

    var w = new WFCMachine<int>(p);
    var nd = new NumericalDomain<int>(new List<Interval<int>>() { new Interval<int>(1, 9) });
    w.CreateEuclideanSpace((9, 9, 1), new DiscreteDomain<int>(domain));

    #region Field init  
    SuperpositionLayer<int>.Collapse(w[(0, 0, 0)]!, 8, true);

    SuperpositionLayer<int>.Collapse(w[(2, 1, 0)]!, 3, true);
    SuperpositionLayer<int>.Collapse(w[(3, 1, 0)]!, 6, true);

    SuperpositionLayer<int>.Collapse(w[(1, 2, 0)]!, 7, true);
    SuperpositionLayer<int>.Collapse(w[(4, 2, 0)]!, 9, true);
    SuperpositionLayer<int>.Collapse(w[(6, 2, 0)]!, 2, true);

    SuperpositionLayer<int>.Collapse(w[(1, 3, 0)]!, 5, true);
    SuperpositionLayer<int>.Collapse(w[(5, 3, 0)]!, 7, true);

    SuperpositionLayer<int>.Collapse(w[(4, 4, 0)]!, 4, true);
    SuperpositionLayer<int>.Collapse(w[(5, 4, 0)]!, 5, true);
    SuperpositionLayer<int>.Collapse(w[(6, 4, 0)]!, 7, true);

    SuperpositionLayer<int>.Collapse(w[(3, 5, 0)]!, 1, true);
    SuperpositionLayer<int>.Collapse(w[(7, 5, 0)]!, 3, true);

    SuperpositionLayer<int>.Collapse(w[(2, 6, 0)]!, 1, true);
    SuperpositionLayer<int>.Collapse(w[(7, 6, 0)]!, 6, true);
    SuperpositionLayer<int>.Collapse(w[(8, 6, 0)]!, 8, true);

    SuperpositionLayer<int>.Collapse(w[(2, 7, 0)]!, 8, true);
    SuperpositionLayer<int>.Collapse(w[(3, 7, 0)]!, 5, true);
    SuperpositionLayer<int>.Collapse(w[(7, 7, 0)]!, 1, true);

    SuperpositionLayer<int>.Collapse(w[(1, 8, 0)]!, 9, true);
    SuperpositionLayer<int>.Collapse(w[(6, 8, 0)]!, 4, true);
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
            await Task.Delay(0);
        }).GetAwaiter().GetResult();
    }
}

void PrintSudoku<T>(MachineState<T> s, int size)
{
    string result = "";
    int space = (int)Math.Sqrt(size);
    for (int i = 0; i < size; i++)
    {
        for (int j = 0; j < size; j++)
        {
            var a = s[v => v.Layers.With<EuclideanLayer<T>>() is var layer && layer.X == j && layer.Y == i].Result.FirstOrDefault();

            var b = SuperpositionLayer<T>.For(a).State != SuperpositionState.Uncertain ? a.Value.Value.ToString() : "_";


            result += $"{b} ";

            if ((j + 1) % space == 0)
            {
                result += $" ";
            }
        }

        result += "\n";

        if ((i + 1) % space == 0)
        {
            result += "\n";
        }
    }

    Console.Write(result);
}

void Maze()
{
    List<string> domain = new() { "╬", "║", "═", "╔", "╗", "╚", "╝", "╠", "╣", "╩", "╦", " " };

    HashSet<string> leftConn = new() { "╬", "═", "╔", "╚", "╠", "╩", "╦" };
    HashSet<string> leftWall = new() { "║", "╗", "╝", "╣", " " };

    HashSet<string> rightConn = new() { "╬", "═", "╗", "╝", "╣", "╩", "╦" };
    HashSet<string> rightWall = new() { "║", "╚", "╔", "╠", " " };

    HashSet<string> topConn = new() { "╬", "║", "╗", "╔", "╣", "╠", "╦" };
    HashSet<string> topWall = new() { "═", "╚", "╝", "╩", " " };

    HashSet<string> bottomConn = new() { "╬", "║", "╚", "╝", "╣", "╠", "╩" };
    HashSet<string> bottomWall = new() { "╔", "╗", "═", "╦", " " };

    List<IQConstraint<string>> mazeRules = new();

    IQConstraint<string> CreateRule(string tile, HashSet<string> left, HashSet<string> right, HashSet<string> front, HashSet<string> back)
    {
        return QSL.Constraint<string>()
            .When(Filters.EqualsToValue(tile))
            .Where(QSL.VonNeumann(new EuclideanConstraintParameter<string>()
            {
                Left = left,
                Right = right,
                Front = front,
                Back = back,
            }))
            .Build();
    }

    mazeRules.Add(CreateRule("╬", leftConn, rightConn, topConn, bottomConn));
    mazeRules.Add(CreateRule("║", leftWall, rightWall, topConn, bottomConn));
    mazeRules.Add(CreateRule("═", leftConn, rightConn, topWall, bottomWall));
    mazeRules.Add(CreateRule("╔", leftWall, rightConn, topWall, bottomConn));
    mazeRules.Add(CreateRule("╗", leftConn, rightWall, topWall, bottomConn));
    mazeRules.Add(CreateRule("╚", leftWall, rightConn, topConn, bottomWall));
    mazeRules.Add(CreateRule("╝", leftConn, rightWall, topConn, bottomWall));
    mazeRules.Add(CreateRule("╠", leftWall, rightConn, topConn, bottomConn));
    mazeRules.Add(CreateRule("╣", leftConn, rightWall, topConn, bottomConn));
    mazeRules.Add(CreateRule("╦", leftConn, rightConn, topWall, bottomConn));
    mazeRules.Add(CreateRule("╩", leftConn, rightConn, topConn, bottomWall));
    mazeRules.Add(CreateRule(" ", leftWall, rightWall, topWall, bottomWall));

    var p = new QMachineParameter<string>()
    {
        Constraints = new()
        {
            GeneralConstraints = mazeRules
        },
        Random = new Random(10)
    };

    WFCMachine<string> w = new WFCMachine<string>(p);

    w.CreateEuclideanSpace((25, 25, 1), new DiscreteDomain<string>(domain));

    foreach (var variable in w.State.Field)
    {
        var localDomain = SuperpositionLayer<string>.For(variable).Domain as DiscreteDomain<string>;
        localDomain.UpdateWeight(" ", 21);
        localDomain.UpdateWeight("╣", 51);
    }

    foreach (var state in w.States)
    {
        Console.Clear();
        Print(state, 25);
    }
}


void Print<T>(MachineState<T> s, int size, bool correctionIndent = false)
{
    string result = "";
    for (int i = 0; i < size; i++)
    {
        for (int j = 0; j < size; j++)
        {
            var a = s[v => v.Layers.With<EuclideanLayer<T>>() is var layer && layer.X == j && layer.Y == i].Result.FirstOrDefault();

            var b = SuperpositionLayer<T>.For(a).State != SuperpositionState.Uncertain ? a.Value.Value.ToString() : "@";

            result += $"{b}{(correctionIndent ? '\u2009' : "")}";
        }

        result += "\n";
    }

    Console.Write(result);
}

void EightQueens()
{
    var domain = new DiscreteDomain<char>(new List<char>() { 'Q', '.' });

    var p = new QMachineParameter<char>
    {
        Constraints = new()
        {
            GeneralConstraints =  new()
            {
                QSL.Constraint<char>()
                    .When(Filters.EqualsToValue('Q'))
                    .Where(QSL.WithLayer<char, EuclideanLayer<char>>(
                        l1 =>
                              QPredicate<char>.Create<EuclideanLayer<char>>(l2 => l1.X == l2.X)
                            | QPredicate<char>.Create<EuclideanLayer<char>>(l2 => l1.Y == l2.Y)
                            | QPredicate<char>.Create<EuclideanLayer<char>>(l2 => Math.Abs(l1.X - l2.X) == Math.Abs(l1.Y - l2.Y))))
                    .Propagate(Propagators.DomainIntersectionWithHashSet<char>(new HashSet<char> { '.' }))
                    .Build()
            },
            ValidationConstraints = new()
            {   
                QSL.Constraint<char>()
                    .Execute(field =>
                        field.Where(Filters.EqualsToValue('Q').ApplyTo).Count()
                        + ~Operations.Comparison(8, COperator.EQ)
                        + ~Propagators.FromBool(true))
                    .Build(),
            }
        }
    };

    var wfc = new WFCMachine<char>(p);
    wfc.CreateEuclideanSpace((8, 8, 1), domain);

    foreach (var state in wfc.States)
    {
        Console.Clear();
        Print(state, 8, true);
    }
}

void RotationExample(int s)
{
    EuclideanBlockTemplate<string> Tile_Grass_cube = new EuclideanBlockTemplate<string>(nameof(Tile_Grass_cube));
    Tile_Grass_cube.Add(Side.Left, "Open");
    Tile_Grass_cube.Add(Side.Front, "Open");
    Tile_Grass_cube.Add(Side.Right, "Open");
    Tile_Grass_cube.Add(Side.Back, "Open");

    EuclideanBlockTemplate<string> Cliff_corner_inside = new EuclideanBlockTemplate<string>(nameof(Cliff_corner_inside));
    Cliff_corner_inside.Add(Side.Left, "Open");
    Cliff_corner_inside.Add(Side.Front, "Open");
    Cliff_corner_inside.Add(Side.Right, "Closed");
    Cliff_corner_inside.Add(Side.Back, "Closed");

    EuclideanBlockTemplate<string> Cliff_Corner_outside = new EuclideanBlockTemplate<string>(nameof(Cliff_Corner_outside));
    Cliff_Corner_outside.Add(Side.Left, "Closed");
    Cliff_Corner_outside.Add(Side.Front, "Closed");
    Cliff_Corner_outside.Add(Side.Right, "Open");
    Cliff_Corner_outside.Add(Side.Back, "Open");

    EuclideanBlockTemplate<string> Cliff_Edge_1 = new EuclideanBlockTemplate<string>(nameof(Cliff_Edge_1));
    Cliff_Edge_1.Add(Side.Left, "Open");
    Cliff_Edge_1.Add(Side.Front, "Open");
    Cliff_Edge_1.Add(Side.Right, "Open");
    Cliff_Edge_1.Add(Side.Back, "Closed");

    var blocks = EuclideanRotationHelper.GenerateConnections<string>(new List<EuclideanBlockTemplate<string>> { Tile_Grass_cube, Cliff_corner_inside, Cliff_Corner_outside });

    List<EuclideanBlock<string>> domain = new();
    List<IQConstraint<EuclideanBlock<string>>> rotationRules = new();
    WFCMachine<EuclideanBlock<string>> machine;

    foreach (var block in blocks)
    {
        domain.Add(block.Key);
        rotationRules.Add(
            QSL.Constraint<EuclideanBlock<string>>()
                .When(Filters.EqualsToValue(block.Key))
                .Where(QSL.VonNeumann(block.Value))
                .Build());
    }

    var p = new QMachineParameter<EuclideanBlock<string>>()
    {
        Constraints = new()
        {
            GeneralConstraints = rotationRules
        },
        Random = new Random(100)
    };

    machine = new(p);

    machine.CreateEuclideanSpace((s, s, 1), new DiscreteDomain<EuclideanBlock<string>>(domain));
    int i = 0;
    foreach (var state in machine.States)
    {
        if (i % 5 == 0)
        {
            Console.WriteLine($"{i}:{machine.State.CurrentState}");
        }
        i++;
    }
}
