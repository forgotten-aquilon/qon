<h1 align="center">「qon」</h1>

[![NuGet](https://img.shields.io/nuget/v/qon.svg)](https://www.nuget.org/packages/qon/)
[![netstandard 2.1](https://img.shields.io/badge/netstandard-2.1-blue)](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-1)

「qon」 is aт Open Source C# library for iterative backtracking-based problem solving. 

## Installation

#### NuGet
```
dotnet add package qon
dotnet add package qon.Spatial
```

#### Unity3D
Install [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity) and use packages listed above.


## What it is, what it isn't and what it can become

**Application:**「qon」 provides API for solving tasks defined by a finite set of variables and rules, how these variables should be evaluated, changed, validated and etc. 
Originally it was created as a base for implementation of the [Wave-Function Collapse](https://github.com/mxgmn/WaveFunctionCollapse) algorithm. Later I decided to implement more generic system.
Despite that main focus of this library is the procedural generation, which means some kinds of tasks are not optimized at all. For example, any math or logic-kind tasks, such as [SEND+MORE=MONEY](https://github.com/forgotten-aquilon/qon/blob/master/Examples/SendMoreMoneyExample.cs), are solvable by 「qon」, but it can take a lot of time.

**Functionality:** Out-of-the-box 「qon」provides functionality to solve tasks using constraint and/or genetic approach.

**Opennes:** Whole library heavily utilizes Interfaces and is designed in such way, you can swap and reimplement almost all of provided classes.

**Aim:** I develop 「qon」with Unity3D in mind, so I'm bound by available runtime. Until Unity embraces CoreCLR in its LTS version I'll not go beyond alpha state for the library. What I want to implement for beta at least: 

- [ ] Proper Numerical Domains utilizing INumber&lt;T&gt;
- [ ] User-side heuristics for optimization fine-tuning
- [ ] Support for hexagonal spaces
- [ ] Feature rich QSL

While developing I was additionally inspired by [MarkovJunior](https://github.com/mxgmn/MarkovJunior), another project of [Maxim Gumin](https://github.com/mxgmn/)(author of WFC), so I utilized some ideas from it. Besides my appreciation to both these projects and their author, I want to highlight that my goal is not to somehow compete or even replace them, but to create its own things. My library is very modular and tries to encompass several different approaches, which unfortunately leads to not-so-good performance — tools created for specific situations always will be better than jack-of-all-trades multitools.
    
## Usage examples

### Simplest Example. Generates 4 variables with different `char` values

```csharp
var domain = DomainHelper.SymbolicalDomain(
    new DomainHelper.CharDomainOptions()
        .WithAlphabet('a', 'j'));

var machine = QMachine<char>.Create()
    .WithConstraintLayer(new()
    {
        GeneralConstraints = new()
        {
            Constraints.CreateConstraint<char>()
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



### Weasel genetic algorithm

[Weasel program - Wikipedia](https://en.wikipedia.org/wiki/Weasel_program)

[Evolutionary algorithm - Rosetta Code](https://rosettacode.org/wiki/Evolutionary_algorithm)

```csharp
var target = "ME THINKS IT IS LIKE A WEASEL";

var machine = QMachine<char>.Create()
    .WithMutation(new MutationLayerParameter<char>
    {
        MutationFunction = Mutations.CreateMutation<char>()
            .Sampling(100)
            .AddMutation(Mutations.Mutation<char>()
                .Frequency(0.1)
                .When(Filters.All<char>())
                .Into(Mutations.RandomFromDomain<char>())
                .Build())
            .Build(),
        Fitness = (field) => Score(field, target)
    })
    .GenerateField((target.Length, 1, 1), 'A');

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

How different AI instruments were used while developing this library:

- Core project
  
  0. All code which is pushed to GitHub repository was manually written by me.
  1. In most cases I used Codex to review my code and to find bugs. It did a pretty good job. It was not asked to produce fixes, only to find particular issues and maybe suggest what can be done. At the end it was me, who wrote these fixes.
  2. In some cases I asked it to write some dummy code, which allowed me to focus on other parts. Later all AI-generated code was manually rewritten. 
     1. For example, when I was rewriting [V1](https://github.com/forgotten-aquilon/qon/tree/V1) version I started 3 times from the scratch, because supposed changes were too big to implement at once, so I used Codex to do some fixes all across the project to let it be compiled at least, then I focused on some particular things and rewrote everything.
     2. Another example, when I was playing with [Rotation support](https://github.com/forgotten-aquilon/qon/blob/master/qon.Spatial/Functions/Constraints/CartesianRotationHelper.cs) I was not sure about how to implement it, so I described the algorithm and asked Codex to produce the code. This code worked as a proof-of-concept for me, so I rewrote it on my own.

- Examples
  
  - Some examples were initially AI-generated, because I wondered how LLM can play with my own code to produce something new. Basically I just asked AI to write whatever it can using my library and I picked couple of good examples, which were later rewritten due to all API changes. The best example is the [Eight Queens](https://github.com/forgotten-aquilon/qon/blob/master/Examples/EightQueensExample.cs)([Wiki](https://en.wikipedia.org/wiki/Eight_queens_puzzle)), it wasn't working properly, because at that moment Validation was not implemented yet, which forced me to do it.

- Tests
  
  - Writing tests is not my strong suit, so most of them are generated. By I tried to make this process as meaningful as possible: I manually pick pieces of code to be tested, ask AI to search project for use cases of this functionality in my code, then I ask to generate tests covering existing functionality and deduce missing conditions, which should be tested to. Also I use some tools like dotCover to help me with tests.

- Documentation and wiki
  
  0. I was frustrated at the lack of any meaningful tools for Wiki's by GitHub. [Adriantanasa/github-wiki-sidebar](https://github.com/adriantanasa/github-wiki-sidebar) wasn't able to generate Sidebar due to some internal error, so I vibe-coded python script to generate \_Sidebar.md for this wiki. Maybe will rewrite it later.
  1. Everything else is written strictly by me. Hope you can excuse some mistakes, because English is not my native language.
