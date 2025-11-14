#pragma warning disable

using Examples;
using qon.Domains;
using qon.Functions.Anchors;
using qon.Functions.Mutations;
using qon.Functions.Replacers;
using qon.Layers.StateLayers;
using qon.Machines;
using qon.Solvers;
using System;using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using qon;
using qon.Functions.Filters;
using qon.Helpers;

//NumberExample.Run();
//SimplestExample.Run();
//SimpleSudokuExample.Run();
//SudokuExample.Run();
//EverestSudokuExample.Run();
//EightQueensExample.Run();
//RotationExample.Run(22);  
//MazeExample.Run();
//WeaselExample.Run();

//TODO: Refine Parameter
Random r = new Random();
QMachine<char> m = new QMachine<char>(new QMachineParameter<char>{Random = r}, v => new DefaultSolver<char>(v));

m.GenerateField(new DiscreteDomain<char>(' ', '@', '.'), (20,20,1), Optional<char>.Of(' '));

EuclideanStateLayer<char>.With(m.State)[(10, 10, 0)].Value = Optional<char>.Of('@');

List<IAnchor<char>> ls = new List<IAnchor<char>>();
ls.Add(Anchors.VNA(Filters.EqualsToValue('@')));
ls.Add(Anchors.VNA(Filters.EqualsToValue(' ')));
ls.Add(Anchors.VNA(Filters.EqualsToValue(' ')));
DefaultMutation<char> mut = new DefaultMutation<char>(
    new BijectiveReplacer<char>(
        new AnchorManager<char>(ls), 
        new Mutators.ValueMutator<char>('@', '.', '@')
        )
    );

MutationLayer<char>.GetOrCreate(m.State)._parameter = new MutationLayerParameter<char>()
{
    MutationFunction = mut.Execute,
    Fitness = field =>
    {
        return r.Next();
    }
};

foreach (var state in m.States)
{
    Console.Clear();
    Print(state, 20, 20);
}

void Print(MachineState<char> state, int x, int y)
{
    for (int i = 0; i < x; i++)
    {
        for (int j = 0; j < y; j++)
        {
            Console.Write(EuclideanStateLayer<char>.With(state)[(j, i, 0)]?.Value.Value);
        }
        Console.Write('\n');
    }
}
