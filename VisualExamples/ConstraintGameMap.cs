using qon;
using qon.Functions;
using qon.Functions.Constraints;
using qon.Functions.Propagators;
using qon.Layers.StateLayers;
using qon.Layers.VariableLayers;
using qon.Machines;
using qon.QSL;
using qon.Solvers;
using qon.Variables;
using qon.Variables.Domains;
using Raylib_cs;
using static Examples.Visual.VisualHelper;
using Color = Raylib_cs.Color;

namespace Examples.Visual
{
    internal static class ConstraintGameMap
    {
        private static class Tile
        {
            public const char DeepWater = Pixel.BluePixel;
            public const char Shallows = Pixel.CyanPixel;
            public const char Sand = Pixel.YellowPixel;
            public const char Grass = Pixel.GreenPixel;
            public const char Forest = Pixel.DarkGreenPixel;
            public const char Mountain = Pixel.GreyPixel;
            public const char Peak = Pixel.BlackPixel;
        }

        private static readonly HashSet<char> OuterBiome = new()
        {
            Tile.DeepWater,
            Tile.Shallows,
            Tile.Sand
        };

        private static readonly HashSet<char> MiddleBiome = new()
        {
            Tile.Shallows,
            Tile.Sand,
            Tile.Grass,
            Tile.Forest
        };

        private static readonly HashSet<char> InnerBiome = new()
        {
            Tile.Sand,
            Tile.Grass,
            Tile.Forest,
            Tile.Mountain,
            Tile.Peak
        };

        public static void Run()
        {
            var machine = CreateMachine();
            using var solver = machine.Solver;

            Draw(machine, 0);
        }

        private static QMachine<char> CreateMachine(Random? random = null)
        {
            var domain = new DiscreteDomain<char>(
                Tile.DeepWater,
                Tile.Shallows,
                Tile.Sand,
                Tile.Grass,
                Tile.Forest,
                Tile.Mountain,
                Tile.Peak)
                .SetWeight(Tile.DeepWater, 15)
                .SetWeight(Tile.Shallows, 9)
                .SetWeight(Tile.Sand, 7)
                .SetWeight(Tile.Grass, 20)
                .SetWeight(Tile.Forest, 12)
                .SetWeight(Tile.Mountain, 8)
                .SetWeight(Tile.Peak, 3);

            var machine = QMachine<char>.Create(new()
            {
                Random = random ?? new Random(),
                SolverInit = QSLSolver.DefaultSolver<char>(new DefaultSolver<char>.SolverParameter
                {
                    BackTrackingEnabled = false
                })
            })
            .WithConstraintLayer(new ConstraintLayerParameter<char>
            {
                GeneralConstraints = CreateTerrainConstraints()
            })
            .GenerateField((Settings.GridSize, Settings.GridSize, 1), domain);

            CollapseBorder(machine);
            SeedBiomeAnchors(machine);

            return machine;
        }

        private static void DrawTerrain(MachineState<char> state)
        {
            var layer = CartesianStateLayer<char>.On(state);

            for (int y = 0; y < Settings.GridSize; y++)
            {
                for (int x = 0; x < Settings.GridSize; x++)
                {
                    var cell = layer[(x, y, 0)];
                    var color = cell?.Value.TryGetValue(out var value) == true
                        ? ResolveColor(value)
                        : Color.LightGray;

                    Raylib.DrawRectangle(x * Settings.PixelSize, y * Settings.PixelSize, Settings.PixelSize, Settings.PixelSize, color);
                }
            }
        }

        private static List<IPreparation<char>> CreateTerrainConstraints()
        {
            return new List<IPreparation<char>>
            {
                CreateAdjacencyRule(Tile.DeepWater, Tile.DeepWater, Tile.Shallows),
                CreateAdjacencyRule(Tile.Shallows, Tile.DeepWater, Tile.Shallows, Tile.Sand),
                CreateAdjacencyRule(Tile.Sand, Tile.Shallows, Tile.Sand, Tile.Grass),
                CreateAdjacencyRule(Tile.Grass, Tile.Sand, Tile.Grass, Tile.Forest, Tile.Mountain),
                CreateAdjacencyRule(Tile.Forest, Tile.Grass, Tile.Forest, Tile.Mountain, Tile.Peak),
                CreateAdjacencyRule(Tile.Mountain, Tile.Grass, Tile.Forest, Tile.Mountain, Tile.Peak),
                CreateAdjacencyRule(Tile.Peak, Tile.Forest, Tile.Mountain, Tile.Peak),
                CreateBiomeBandConstraint()
            };
        }

        private static IPreparation<char> CreateAdjacencyRule(char tile, params char[] allowedNeighbors)
        {
            var left = new HashSet<char>(allowedNeighbors);
            var right = new HashSet<char>(allowedNeighbors);
            var front = new HashSet<char>(allowedNeighbors);
            var back = new HashSet<char>(allowedNeighbors);

            return Constraints.CreateConstraint<char>()
                .When(Filters.EqualsToValue(tile))
                .Where(Cartesian.VonNeumann(new CartesianConstraintParameter<char>
                {
                    CenterLevel =
                    {
                        Left = left,
                        Right = right,
                        Front = front,
                        Back = back
                    }
                }))
                .Build();
        }

        private static IPreparation<char> CreateBiomeBandConstraint()
        {
            return Constraints.CreateConstraint<char>()
                .Select(Filters.All<char>())
                .Propagate(new Propagator<char>(variables =>
                {
                    var changes = 0;
                    var centerX = (Settings.GridSize - 1) / 2.0;
                    var centerY = (Settings.GridSize - 1) / 2.0;

                    foreach (var variable in variables)
                    {
                        if (variable.OnDomainLayer().State != ValueState.Uncertain)
                        {
                            continue;
                        }

                        var layer = CartesianLayer<char>.On(variable);
                        var dx = layer.X - centerX;
                        var dy = layer.Y - centerY;
                        var distance = Math.Sqrt(dx * dx + dy * dy);

                        var allowedTiles = distance >= 10.5
                            ? OuterBiome
                            : distance >= 6.5
                                ? MiddleBiome
                                : InnerBiome;

                        changes += DomainHelper<char>.DomainIntersectionWithHashSet(variable, allowedTiles);
                        changes += ConstraintLayer<char>.TryCollapseVariable(variable).HasValue ? 1 : 0;
                    }

                    return Result.Success(changes);
                }))
                .Build();
        }

        private static void CollapseBorder(QMachine<char> machine)
        {
            for (int x = 0; x < Settings.GridSize; x++)
            {
                machine.At(x, 0, 0).Value = Tile.DeepWater;
                machine.At(x, Settings.GridSize - 1, 0).Value = Tile.DeepWater;
            }

            for (int y = 0; y < Settings.GridSize; y++)
            {
                machine.At(0, y, 0).Value = Tile.DeepWater;
                machine.At(Settings.GridSize - 1, y, 0).Value = Tile.DeepWater;
            }
        }

        private static void SeedBiomeAnchors(QMachine<char> machine)
        {
            var centerX = Settings.GridSize / 2;
            var centerY = Settings.GridSize / 2;

            machine.At(centerX, centerY, 0).Value = Tile.Peak;
            machine.At(centerX - 3, centerY + 2, 0).Value = Tile.Forest;
            machine.At(centerX + 4, centerY - 1, 0).Value = Tile.Grass;
        }
    }
}
