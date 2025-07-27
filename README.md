# qon

qon is a C# constraint programming library with a primary focus on the Wave-Function Collapse algorithm.

## How to use it

This library was created for usage in Unity3D but also doesn't have any specific dependencies, so it can be used anywhere where you can run C# code.

Because Unity3D still does not support the latest .NET versions (therefore the latest C# versions), I decided to target .NET 5.0 with a later update to a more relevant version in mind. Until I don't want to bother myself with creating and maintaining NuGet packages, so just copy src folder into your project and use qon namespace.

## Terminology

* Variables — objects of the `SuperpositionVariable<T>` class. Variables can be defined and undefined. A defined variable has a value of `T` type, and an undefined variable has a domain of `T` type values.

* Field of variables — all variables of a specific solution machine.

* Domain — objects of classes implementing the `IDomain<T>` interface. Encapsulate mechanisms to work with sets of potential variable values.

* Collapse — process of reducing a variable's domain to a single value and making it defined.

* Solution machine — object of `QMachine<T>` or derived class. It is used for solving problems defined with rules and variables.

* Rules — objects of classes implementing `IGlobalRule<T>` or `ILocalRule<T>` interfaces. Global Rules are applied to the whole field of variables. Global Rules are defined by Aggregators and Filters. Local Rules are applied to some subset of a field based on specific variables. Local Rules are defined by Guards, Aggregators and Filters.

* Aggregators — objects of classes `GroupingAggregator<T>` or `SelectingAggregator<T>`. Grouping Aggregators take variables and group them by specified function. Selecting Aggregators select a subset of variables.

* Filters — objects of class `Filter<T>`. Filters check variables prepared by Aggregators applying some function and return Constraint Result.

* Guards — objects of class `Guard<T>`. Guards are used for the definition of Local Rules. They check can this specific rule be applied based on a certain variable.

* Constraint Result — objects of `ConstraintResult` struct. They are returned as a result of Filter application. `ConstraintResult` has a `PropagationOutcome` enum property, which states how successful the filter was applied.

## Examples && Explanation

You can find all these examples in the Program.cs file.

### Simplest Example

```C#
var parameter = new QMachineParameter<char>()
{
    GlobalRules = new()
    {

        new GlobalRule<char>(Aggregators.All<char>(), Filters.AllDistinct<char>())
    },
};

var machine = new QMachine<char>(parameter);

List<char> letters = new() { 'a', 'b', 'c', 'd', 'e', 'f', 'g' };

machine.GenerateField(new DiscreteDomain<char>(letters), new []{"V1", "V2", "V3", "V4"});

foreach (var state in machine.States)
{
    Console.WriteLine(state);
}
```

To solve a problem, you need to set variables. Variables are undefined by default and have a domain — set of values this variable can possibly become.

`QMachineParameter<T>` is a generic class, which is used for initialisation. Parameter `T` states the type of variables.

In this example we set only `GlobalRules`. These rules are applied to all variables. 

```C#
new GlobalRule<char>(Aggregators.All<char>(), Filters.AllDistinct<char>())
```

Global rules are created based on Aggregator and Filter. Aggregators apply some functions to select a subset of variables. Filters check these variables for compliance with specified function and remove unnecessary values from domains.

Here we define a single rule with aggregator `Aggregators.All<char>()` which takes all variables at once, and filter `Filters.AllDistinct<char>()` which states all variables should have different values.

```C#
var machine = new QMachine<char>(parameter);
```

We initialise the solution machine with a parameter. The problem is described by the single rule, but we need variables to apply this rule.

```C#
List<char> letters = new() { 'a', 'b', 'c', 'd', 'e', 'f', 'g' };
machine.GenerateField(new DiscreteDomain<char>(letters), new []{"V1", "V2", "V3", "V4"});
```

We generate a field of variables based on a domain consisting of several English letters and a list of names for these variables.

```C#
foreach (var state in machine.States)
{
    Console.WriteLine(state);
}
```

The problem is solved not in a single function like `Solve()`, but in a finite, yet unknowingly large foreach cycle. Each iteration new field of variable is lazily calculated based on applied rules. After applying rules, the solution machine picks a random undefined variable and collapses it based on its domain. It repeats until the problem is solved or it's impossible to solve. In case the collapse of a variable leads to a dead end, the solution machine uses backtracking to return to a previous state, removing the wrong value from the domain of the variable.

### 4x4 Sudoku

```C#
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
```

We define new Global Rules.

```C#
new GlobalRule<int>(Aggregators.GroupByTag<int>("x"), Filters.AllDistinct<int>()),
```

Aggregators `GroupByTag<int>("x")` and `GroupByTag<int>("y")` group variables based on similar values of `X` and `Y` properties.

`EuclideanAggregators.GroupByRectangle<int>(2,2)` is a special Aggregator used in 2D space based on `X` and `Y` properties. It divides variables represented by a 2D field by rectangles of a specified width and height starting from the top-right corner.

Filter `AllDistinct<int>()` checks if variables of groups created by Aggregators have different values in each group separately.

`WFCMachine<T>` is a solution machine optimised to work with variables, supposed to have spatial properties.

```C#
w.CreateEuclideanSpace((4, 4, 1), new DiscreteDomain<int>(domain));
```

We generate variables in a Euclidean 3D space. 4 variables per `X` dimension, 4 variables per `Y` dimension, and 1 variable per `Z` dimension, so 16 variables at all.

### Normal Sudoku and Everest Sudoku

Everything is the same, but now we work with a 9x9x1 field, and we define some variables with initial values before solving it.

```C#
w[0, 0].Collapse(8, true);

w[2, 1].Collapse(3, true);
w[3, 1].Collapse(6, true);

w[1, 2].Collapse(7, true);
w[4, 2].Collapse(9, true);
w[6, 2].Collapse(2, true);
...
```

We get specific variables by their coordinates, collapse them with values we want, and mark them as constants (other variables collapsed in the solution will be marked as defined. It can be used for easier field analysis).

### 2D maze

```C#
List<string> domain = new(){ "╬", "║", "═", "╔", "╗", "╚", "╝", "╠", "╣", "╩", "╦", " " };

List<string> leftConn = new(){ "╬", "═", "╔", "╚", "╠", "╩", "╦" };
List<string> leftWall = new(){ "║", "╗", "╝", "╣", " " };

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
```

```C#
List<string> domain = ...
```

First we define the domain for all variables.

```C#
List<string> leftConn = new(){ "╬", "═", "╔", "╚", "╠", "╩", "╦" };
List<string> leftWall = new(){ "║", "╗", "╝", "╣", " " };
...
```

Then we separately define subsets of a domain, which will be used in Rule defenitions.

```C#

new EuclideanRule<string>(new(){Guards.Equals("╬")},
    new EuclideanRuleParameter<string>(){
        Left = leftConn,
        Right = rightConn,
        Front = topConn,
        Back = bottomConn,
    }),
```

We define Local Rules for each possible value. In other words, the solution machine will check already collapsed variables and, based on spatial proximity, reduce domains of closest undefined variables based on the assigned Local Rule.

`EuclideanRule` are special rules that define possible values of variables on left, on front, on right, on back, on top and on bottom of a specific defined variable.

In this example we define the rule for a variable with a value of `╬`.

```C#
variable.Domain.UpdateWeight(" ", 5);
```

Additionally, we change the probability weight of a ` ` value, increasing it by 5 (the base probability weight is 1).