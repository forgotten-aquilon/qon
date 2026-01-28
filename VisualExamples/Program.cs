using System;
using System.Collections.Generic;
using System.Drawing;
using Raylib_cs;
using qon;
using qon.Functions.Anchors;
using qon.Functions.Filters;
using qon.Functions.Mutations;
using qon.Functions.Replacers;
using qon.Helpers;
using qon.Layers.StateLayers;
using qon.Machines;
using qon.Variables.Domains;
using Color = Raylib_cs.Color;

const int GridSize = 30;
const int PixelSize = 5;
const int CanvasSize = GridSize * PixelSize;
const int InfoPanelHeight = 40;
const char BlackPixel = '@';
const char WhitePixel = ' ';

var random = new Random(42);
var machine = CreateMachine(random);

using var solver = machine.Solver;
bool simulationFinished = !solver.MoveNext();
int iteration = 0;

Raylib.InitWindow(CanvasSize, CanvasSize + InfoPanelHeight, "Anchor Expansion Visual");

try
{
    while (!Raylib.WindowShouldClose())
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.White);

        DrawField(machine.State);

        var statusText = simulationFinished ? "Finished" : "Running";
        Raylib.DrawText($"{statusText} · Iteration {solver.StepCounter}", 10, CanvasSize + 8, 20, Color.Black);

        Raylib.EndDrawing();

        if (simulationFinished)
        {
            continue;
        }

        var moved = solver.MoveNext();

        if (!moved)
        {
            simulationFinished = true;
        }
    }
}
finally
{
    Raylib.CloseWindow();
}

static QMachine<char> CreateMachine(Random random)
{
    var machine = new QMachine<char>(new QMachineParameter<char> { Random = random });

    machine.GenerateField(new DiscreteDomain<char>(BlackPixel, WhitePixel), (GridSize, GridSize, 1), Optional<char>.Of(WhitePixel));

    var center = GridSize / 2;
    var centerVariable = EuclideanStateLayer<char>.With(machine.State)[(center, center, 0)];
    if (centerVariable is not null)
    {
        centerVariable.Value = Optional<char>.Of(BlackPixel);
    }

    var anchors = new List<IAnchor<char>>
    {
        Anchors.VNA(Filters.EqualsToValue(BlackPixel)),
        Anchors.VNA(Filters.EqualsToValue(WhitePixel))
    };

    var mutation = new DefaultMutation<char>(
        new BijectiveReplacer<char>(
            new AnchorManager<char>(anchors),
            new Mutators.ValueMutator<char>(BlackPixel, BlackPixel)));

    var mutationLayer = MutationLayer<char>.GetOrCreate(machine.State);
    mutationLayer._parameter = new MutationLayerParameter<char>
    {
        MutationFunction = mutation.Execute,
        Fitness = _ => random.Next()
    };

    return machine;
}

static void DrawField(MachineState<char> state)
{
    var layer = EuclideanStateLayer<char>.With(state);

    for (int y = 0; y < GridSize; y++)
    {
        for (int x = 0; x < GridSize; x++)
        {
            var cell = layer[(x, y, 0)];
            var pixelValue = WhitePixel;
            if (cell?.Value.TryGetValue(out var resolvedValue) == true)
            {
                pixelValue = resolvedValue;
            }

            var color = ResolveColor(pixelValue);

            Raylib.DrawRectangle(x * PixelSize, y * PixelSize, PixelSize, PixelSize, color);
        }
    }
}

static Color ResolveColor(char value)
{
    return value == BlackPixel ? Color.Black : Color.White;
}
