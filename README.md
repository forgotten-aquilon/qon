<h1 align="center">「qon」</h1>

qon is a C# library for iterative backtracking-based problem solving. By default it provides functionality for constraint and genetic programming. Originally it was created as a base for implementation of the [Wave-Function Collapse](https://github.com/mxgmn/WaveFunctionCollapse) algorithm. Main idea for its usage is a procedural generation, but of course it can be applied for anything else.

While developing I was additionally inspired by [MarkovJunior](github.com/mxgmn/MarkovJunior), another project of [Maxim Gumin](https://github.com/mxgmn/)(author of WFC), so I utilized some ideas from it. Besides my appreciation to both these projects and their author, I want to highlight that my goal is not to somehow compete or even replace them, but to create its own things. My library is very modular and tries to encompass several different approaches, which unfortunately leads to not-so-good performance — tools created for specific situations always will be better than jack-of-all-trades multitools.

## How to install

This library was created for usage in Unity3D but also doesn't have any specific dependencies, so it can be used anywhere you can run C# code.

Unity3D still does not embraced CoreCLR **=>** it means it doesn't support the latest .NET versions **=>** therefore the latest C# versions, so I decided to target .NET 5.0 with a later update to a more relevant version in mind. At least until I create a proper stable version I don't want to manage nuget package, so if you want to use it, just download the project and copy [/src](https://github.com/forgotten-aquilon/qon/tree/master/src) project into your app folder and user **qon** namespace.

## How to use

I suggest to check the [Wiki](https://github.com/forgotten-aquilon/qon/wiki) to see all documentation, including all examples being explained line-by-line. Couple of specific examples you find below.

### Usage examples

#### Simplest Example. Generates 4 variables with different `char` values

```csharp
var domain = DomainHelper.SymbolicalDomain(
                new DomainHelper.CharDomainOptions()
                    .WithAlphabet('a', 'j'));

var machine = QSL.Machine<char>()
    .WithConstraint(new()
    {
        GeneralConstraints = new()
        {
            QSL.CreateConstraint<char>()
                .Select(Filters.All<char>())
                .Propagate(Propagators.AllDistinct<char>())
                .Build()
        }
    })
    .GenerateField(domain, 10);

foreach (var state in machine.States)
{
    Console.WriteLine($"{state}: {machine.Status}");
}
```



#### Weasel genetic algorithm

[Weasel program - Wikipedia](https://en.wikipedia.org/wiki/Weasel_program)

[Evolutionary algorithm - Rosetta Code](https://rosettacode.org/wiki/Evolutionary_algorithm)

```csharp
var domain = DomainHelper.SymbolicalDomain(new DomainHelper.CharDomainOptions()
        .WithAlphabet('A', 'Z')
        .WithOtherSymbols(' '));

var target = "ME THINKS IT IS LIKE A WEASEL";

var machine = QSL.Machine<char>()
    .WithMutation(new MutationLayerParameter<char>
    {
        MutationFunction = QSL.CreateMutation<char>()
            .Sampling(100)
            .AddMutation(QSL.Mutation<char>()
                .Frequency(0.1)
                .When(Filters.All<char>())
                .Into(Mutations<char>.RandomFromDomain)
                .Build())
            .Build(),
        Fitness = (field) => Score(field, target)
    })
    .GenerateField(domain, (29, 1, 1), 'A');

foreach (var state in machine.States)
{
    Console.WriteLine(FormatState(state));
}

int Score(Field<char> first, string second)
{
    if (first.Count != second.Length)
    {
        return -1;
    }

    int mismatch = 0;
                    
    for (int i = 0; i < first.Count; i++)
    {
        if (!first[i].Value.CheckValue(second[i]))
        {
            mismatch++;
        }
    }

    return mismatch;
}

string FormatState(MachineState<char> state)
{
    return new string(state.Field.Select(variable => variable.Value.Value).ToArray());
}
```

## AI Usage

I consider myself as a mild pro-ai person. I think AI (even LLMs) can be used on a par with other tools. But they should be used **only** with full transparency and its results should be treated with a caution.

How different Ai instruments were used while developing this library:

- Core project
  
  0. All code which is pushed to GitHub repository was manually written by me.
  1. In most cases I used Codex to review my code and to find bugs. It did a pretty good job. It was not asked to produce fixes, only to find particular issues and maybe suggest what can be done. At the end it was me, who wrote fixes.
  2. In some cases I asked it to write some dummy code, which allowed me to focus on other parts. Later all AI-generated code was manually rewritten. 
     1. For example, when I was rewriting [V1](https://github.com/forgotten-aquilon/qon/tree/V1) version I started 3 times from the scratch, because supposed changes were too big to implement at once, so I used Codex to do some fixes all across the project to let it be able to be compiled at least, then I focused on some particular things and rewrote everything.
     2. Another example, when I was playing with [Rotation support](https://github.com/forgotten-aquilon/qon/blob/master/src/Functions/Constraints/EuclideanRotationHelper.cs) I was not sure about how to implement it, so I described the algorithm and asked Codex to produce the code. This code worked as a proof-of-concept for me, so I rewrote it on my own.

- Examples
  
  - Some examples were initially AI-generated, because I wondered how LLM can play with my own code to produce something new. Basically I just asked AI to write whatever it can using my library and I picked couple of good examples, which were later rewritten due to all API changes. The best example is the [Eight Queens](https://github.com/forgotten-aquilon/qon/blob/master/Examples/EightQueensExample.cs)([Wiki](https://en.wikipedia.org/wiki/Eight_queens_puzzle)), it wasn't working properly, because at that moment Validation was not implemented yet, which forced me to do it.

- Tests
  
  - Writing tests is not my strong suit, so almost all of it was generated. By I tried to make this process as meaningful as possible. I manually pick some places to be tested, additionally asked AI to find everything else, what should be tested. Split it into separate classes for testing and only then asked Codex to generate tests. I manually checked all tests and fixed some issues. Also I use some tools like dotCover to help me with tests.

- Documentation and wiki
  
  0. I was frustrated at the lack of any meaningful tools for Wiki's by GitHub. [Adriantanasa/github-wiki-sidebar](https://github.com/adriantanasa/github-wiki-sidebar) wasn't able to generate Sidebar due to some internal error, so I vibe-coded python script to generate \_Sidebar.md for this wiki. Maybe will rewrite it later.
  1. Everything else is written strictly by me. Hope you can excuse some mistakes, because English is not my native language.
