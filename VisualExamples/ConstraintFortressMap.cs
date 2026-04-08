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
    internal static class ConstraintFortressMap
    {
        private static class Tile
        {
            public const char Grass = Pixel.GreenPixel;
            public const char Garden = Pixel.DarkGreenPixel;
            public const char Road = Pixel.YellowPixel;
            public const char Bridge = Pixel.CyanPixel;
            public const char Moat = Pixel.BluePixel;
            public const char Wall = Pixel.GreyPixel;
            public const char Tower = Pixel.BlackPixel;
            public const char Keep = Pixel.RedPixel;
        }

        private static readonly HashSet<char> OuterGround = new()
        {
            Tile.Grass,
            Tile.Road
        };

        private static readonly HashSet<char> MoatBand = new()
        {
            Tile.Moat,
            Tile.Bridge
        };

        private static readonly HashSet<char> WallBand = new()
        {
            Tile.Wall,
            Tile.Tower,
            Tile.Road
        };

        private static readonly HashSet<char> Courtyard = new()
        {
            Tile.Road,
            Tile.Garden
        };

        private static readonly HashSet<char> KeepCore = new()
        {
            Tile.Keep,
            Tile.Road,
            Tile.Garden
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
                Tile.Grass,
                Tile.Garden,
                Tile.Road,
                Tile.Bridge,
                Tile.Moat,
                Tile.Wall,
                Tile.Tower,
                Tile.Keep)
                .SetWeight(Tile.Grass, 18)
                .SetWeight(Tile.Garden, 8)
                .SetWeight(Tile.Road, 10)
                .SetWeight(Tile.Bridge, 3)
                .SetWeight(Tile.Moat, 12)
                .SetWeight(Tile.Wall, 10)
                .SetWeight(Tile.Tower, 4)
                .SetWeight(Tile.Keep, 7);

            var machine = QMachine<char>.Create(new()
            {
                Random = random ?? new Random(123),
                SolverInit = QSLSolver.DefaultSolver<char>(new DefaultSolver<char>.SolverParameter
                {
                    BackTrackingEnabled = false
                })
            })
            .WithConstraintLayer(new ConstraintLayerParameter<char>
            {
                GeneralConstraints = CreateFortressConstraints()
            })
            .GenerateField((Settings.GridSize, Settings.GridSize, 1), domain);

            SeedKeep(machine);
            SeedTowers(machine);
            SeedRoadsAndBridges(machine);

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

        private static List<IPreparation<char>> CreateFortressConstraints()
        {
            return new List<IPreparation<char>>
            {
                CreateAdjacencyRule(Tile.Grass, Tile.Grass, Tile.Road, Tile.Moat, Tile.Bridge, Tile.Garden),
                CreateAdjacencyRule(Tile.Garden, Tile.Grass, Tile.Garden, Tile.Road, Tile.Wall, Tile.Keep),
                CreateAdjacencyRule(Tile.Road, Tile.Grass, Tile.Garden, Tile.Road, Tile.Bridge, Tile.Wall, Tile.Tower, Tile.Keep),
                CreateAdjacencyRule(Tile.Bridge, Tile.Grass, Tile.Road, Tile.Bridge, Tile.Moat, Tile.Wall),
                CreateAdjacencyRule(Tile.Moat, Tile.Grass, Tile.Bridge, Tile.Moat, Tile.Wall),
                CreateAdjacencyRule(Tile.Wall, Tile.Garden, Tile.Road, Tile.Bridge, Tile.Moat, Tile.Wall, Tile.Tower),
                CreateAdjacencyRule(Tile.Tower, Tile.Road, Tile.Bridge, Tile.Moat, Tile.Wall, Tile.Tower),
                CreateAdjacencyRule(Tile.Keep, Tile.Garden, Tile.Road, Tile.Keep),
                CreateRingConstraint()
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

        private static IPreparation<char> CreateRingConstraint()
        {
            return Constraints.CreateConstraint<char>()
                .Select(Filters.All<char>())
                .Propagate(new Propagator<char>(variables =>
                {
                    var changes = 0;
                    var centerX = Settings.GridSize / 2;
                    var centerY = Settings.GridSize / 2;

                    foreach (var variable in variables)
                    {
                        if (variable.OnDomainLayer().State != ValueState.Uncertain)
                        {
                            continue;
                        }

                        var layer = CartesianLayer<char>.On(variable);
                        var ring = Math.Max(Math.Abs(layer.X - centerX), Math.Abs(layer.Y - centerY));

                        var allowedTiles = ring switch
                        {
                            >= 9 => OuterGround,
                            >= 7 => MoatBand,
                            6 => WallBand,
                            >= 3 => Courtyard,
                            _ => KeepCore
                        };

                        changes += DomainHelper<char>.DomainIntersectionWithHashSet(variable, allowedTiles);
                        changes += ConstraintLayer<char>.TryCollapseVariable(variable).HasValue ? 1 : 0;
                    }

                    return Result.Success(changes);
                }))
                .Build();
        }

        private static void SeedKeep(QMachine<char> machine)
        {
            var centerX = Settings.GridSize / 2;
            var centerY = Settings.GridSize / 2;

            for (int y = centerY - 1; y <= centerY + 1; y++)
            {
                for (int x = centerX - 1; x <= centerX + 1; x++)
                {
                    machine.At(x, y, 0).Value = Tile.Keep;
                }
            }
        }

        private static void SeedTowers(QMachine<char> machine)
        {
            var centerX = Settings.GridSize / 2;
            var centerY = Settings.GridSize / 2;
            var towerOffset = 6;

            machine.At(centerX - towerOffset, centerY - towerOffset, 0).Value = Tile.Tower;
            machine.At(centerX + towerOffset, centerY - towerOffset, 0).Value = Tile.Tower;
            machine.At(centerX - towerOffset, centerY + towerOffset, 0).Value = Tile.Tower;
            machine.At(centerX + towerOffset, centerY + towerOffset, 0).Value = Tile.Tower;
        }

        private static void SeedRoadsAndBridges(QMachine<char> machine)
        {
            var centerX = Settings.GridSize / 2;
            var centerY = Settings.GridSize / 2;

            for (int y = 0; y < Settings.GridSize; y++)
            {
                SeedAxisTile(machine, centerX, y, centerX, centerY);
            }

            for (int x = 0; x < Settings.GridSize; x++)
            {
                SeedAxisTile(machine, x, centerY, centerX, centerY);
            }
        }

        private static void SeedAxisTile(QMachine<char> machine, int x, int y, int centerX, int centerY)
        {
            var distance = Math.Max(Math.Abs(x - centerX), Math.Abs(y - centerY));

            var tile = distance switch
            {
                >= 9 => Tile.Road,
                >= 7 => Tile.Bridge,
                6 => Tile.Road,
                >= 3 => Tile.Road,
                _ => Tile.Keep
            };

            machine.At(x, y, 0).Value = tile;
        }
    }
}
