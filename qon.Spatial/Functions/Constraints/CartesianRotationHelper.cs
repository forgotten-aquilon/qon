using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using qon.Helpers;
using qon.QSL;

namespace qon.Functions.Constraints
{
    public class Rotations : IEnumerable<int>
    {
        public HashSet<int> Numbers { get; private set; }
        public Rotations() 
        {
            Numbers = new HashSet<int>(new int[] { 0, 1, 2, 3 });
        }

        public Rotations(IEnumerable<int> rotations)
        {
            Numbers = new HashSet<int>(rotations.Where(x => x >= 0 && x < 4));
        }

        public Rotations Exclude(IEnumerable<int> rotations)
        {
            Numbers.ExceptWith(rotations);
            return this;
        }

        public IEnumerator<int> GetEnumerator()
        {
            return Numbers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public enum ConnectionDirection
    {
        In,                        
        Out,                        
        Both                        
    }

    public struct CornerConnection
    {
        public string Name { private set; get; }
        public ConnectionDirection Dir { private set; get; }

        public CornerConnection(string name, ConnectionDirection dir)
        {
            Name = name;
            Dir = dir;
        }
    }

    public struct SideConnection
    {
        public string Name { private set; get; }
        public ConnectionDirection Dir { private set; get; }
        
        public SideConnection(string name, ConnectionDirection dir)
        {
            Name = name;
            Dir = dir;
        }
    }

    public struct VerticalConnection
    {
        public string Name { private set; get; }
        public ConnectionDirection Dir { private set; get; }
        public Rotations Rotations { private set; get; }

        public VerticalConnection(string name, ConnectionDirection dir)
        {
            Name = name;
            Dir = dir;
            Rotations = new Rotations();
        }

        public VerticalConnection(string name, ConnectionDirection dir, Rotations rotations)
        {
            Name = name;
            Dir = dir;
            Rotations = rotations;
        }
    }

    public class LevelHandler
    {
        public Dictionary<Side, List<SideConnection>> Pools { get; }
        public Dictionary<Corner, List<CornerConnection>> CornerPools { get; }

        public LevelHandler()
        {
            Pools = new Dictionary<Side, List<SideConnection>>
            {
                [Side.Front] = new(),
                [Side.Right] = new(),
                [Side.Back] = new(),
                [Side.Left] = new()
            };

            CornerPools = new Dictionary<Corner, List<CornerConnection>>
            {
                [Corner.FrontLeft] = new(),
                [Corner.FrontRight] = new(),
                [Corner.BackRight] = new(),
                [Corner.BackLeft] = new(),
            };
        }
    }

    public sealed class CartesianBlockTemplate<TQ> where TQ : notnull
    {
        public TQ Value { get; }

        public bool SelfConnection { get; }
        public Rotations Rotations { get; }

        public Dictionary<Level, LevelHandler> Levels { get; }
        public Dictionary<Slab, List<VerticalConnection>> VerticalPools { get; }

        public CartesianBlockTemplate(TQ value, Rotations rotations, bool selfConnection = true)
        {
            Value = value;

            Levels = new Dictionary<Level, LevelHandler>
            {
                [Level.Top] = new(),
                [Level.Middle] = new(),
                [Level.Bottom] = new(),
            };

            VerticalPools = new Dictionary<Slab, List<VerticalConnection>>()
            {
                [Slab.Top] = new(),
                [Slab.Bottom] = new(),
            };

            SelfConnection = selfConnection;

            Rotations = rotations;
        }

        public CartesianBlockTemplate(TQ value, bool selfConnection = true) : this(value, new Rotations(), selfConnection) { }

        public CartesianBlockTemplate<TQ> Add(Level level, Side side, string connName, ConnectionDirection dir = ConnectionDirection.Both)
        {
            Levels[level].Pools[side].Add(new SideConnection(connName, dir));
            return this;
        }

        public CartesianBlockTemplate<TQ> Add(Level level, Corner corner, string connName, ConnectionDirection dir = ConnectionDirection.Both)
        {
            Levels[level].CornerPools[corner].Add(new CornerConnection(connName, dir));
            return this;
        }

        public CartesianBlockTemplate<TQ> Add(Slab slab, string connName, ConnectionDirection dir = ConnectionDirection.Both)
        {
            VerticalPools[slab].Add(new VerticalConnection(connName, dir));
            return this;
        }

        public CartesianBlock<TQ> ToCartesianBlock(int rot)
        {
            return new CartesianBlock<TQ>(Value, rot);
        }
    }

    public struct CartesianBlock<TQ> where TQ : notnull
    {
        public TQ Value { private set; get; }
        public int Rotation { private set; get; }

        public CartesianBlock(TQ value, int rotation)
        {
            Value = value;
            Rotation = rotation;
        }
    }

    public static class CartesianRotationHelper
    {
        public static Dictionary<CartesianBlock<TQ>, CartesianConstraintParameter<CartesianBlock<TQ>>> GenerateConnections<TQ>(List<CartesianBlockTemplate<TQ>> blocks)
            where TQ : notnull
        {
            Dictionary<CartesianBlock<TQ>, CartesianConstraintParameter<CartesianBlock<TQ>>> parameters = new();

            CartesianBlockTemplate<TQ> block1;
            CartesianBlockTemplate<TQ> block2;

            //Iterating first block of T value
            for (int b1 = 0; b1 < blocks.Count; b1++)
            {
                block1 = blocks[b1];

                //Iterating second block of T value
                for (int b2 = 0; b2 < blocks.Count; b2++)
                {
                    block2 = blocks[b2];

                    //Optimization: Skipping mirror cases
                    if (b1 < b2)
                        continue;

                    if (b1 == b2 && !block1.SelfConnection)
                        continue;

                    //Rotating first block
                    foreach (var rot1 in block1.Rotations)
                    {
                        //Rotating second block
                        foreach (var rot2 in block2.Rotations)
                        {
                            //Checking all horizontal levels of cubic connections
                            foreach (var level in CartesianExtensions.Levels)
                            {
                                CheckSideCompatibility(parameters, level, (block1, rot1), (block2, rot2));
                                CheckCornerCompatibility(parameters, level, (block1, rot1), (block2, rot2));
                            }

                            CheckVerticalCompatibility(parameters, (block1, rot1), (block2, rot2));
                        }
                    }
                }
            }

            return parameters;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckSideCompatibility<TQ>(Dictionary<CartesianBlock<TQ>, CartesianConstraintParameter<CartesianBlock<TQ>>> parameters, Level level,
            (CartesianBlockTemplate<TQ> block, int rot) item1, (CartesianBlockTemplate<TQ> block, int rot) item2)
            where TQ : notnull
        {
            //Iterating over opposite sides of both blocks
            //"side" is the side of the first block
            foreach (Side side in CartesianExtensions.Sides)
            {
                CheckSideConnection(parameters, level, side, item1, item2);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckSideConnection<TQ>(Dictionary<CartesianBlock<TQ>, CartesianConstraintParameter<CartesianBlock<TQ>>> parameters, Level level, Side side, 
            (CartesianBlockTemplate<TQ> block, int rot) item1, (CartesianBlockTemplate<TQ> block, int rot) item2)
            where TQ : notnull
        {
            //"oppositeSide" is the side of the second block, which can be potentially connected to the side of the first block
            Side oppositeSide = (Side)(((int)side + 2) % 4);

            var rotatedBlock1 = RotateSides(item1.block, item1.rot, level);
            //Iterating over available connections of the first block
            foreach (SideConnection conn1 in rotatedBlock1[side])
            {
                Dictionary<Side, List<SideConnection>> rotatedBlock2 = RotateSides(item2.block, item2.rot, level);
                //Iterating over available connections of the second block
                foreach (SideConnection conn2 in rotatedBlock2[oppositeSide])
                {
                    if (conn1.Name == conn2.Name && IsDirectionsCompatible(conn1.Dir, conn2.Dir))
                    {
                        CartesianBlock<TQ> keyBlock1 = item1.block.ToCartesianBlock(item1.rot);
                        CartesianBlock<TQ> keyBlock2 = item2.block.ToCartesianBlock(item2.rot);

                        CartesianConstraintParameter<CartesianBlock<TQ>> r1 = parameters.TryGetOrCreate(keyBlock1);
                        r1[level][side].Add(keyBlock2);

                        CartesianConstraintParameter<CartesianBlock<TQ>> r2 = parameters.TryGetOrCreate(keyBlock2);
                        r2[level][oppositeSide].Add(keyBlock1);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckCornerCompatibility<TQ>(Dictionary<CartesianBlock<TQ>, CartesianConstraintParameter<CartesianBlock<TQ>>> parameters, Level level,
            (CartesianBlockTemplate<TQ> block, int rot) item1, (CartesianBlockTemplate<TQ> block, int rot) item2)
            where TQ : notnull
        {
            //Iterating over opposite corners of both blocks
            //"corner" is the corner of the first block
            foreach (Corner corner in CartesianExtensions.Corners)
            {
                CheckCornerConnection(parameters, level, corner, item1, item2);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckCornerConnection<TQ>(Dictionary<CartesianBlock<TQ>, CartesianConstraintParameter<CartesianBlock<TQ>>> parameters, Level level, Corner corner,
            (CartesianBlockTemplate<TQ> block, int rot) item1, (CartesianBlockTemplate<TQ> block, int rot) item2)
            where TQ : notnull
        {
            //"oppositeCorner" is the corner of the second block, which can be potentially connected to the corner of the first block
            Corner oppositeCorner = (Corner)(((int)corner + 2) % 4);

            Dictionary<Corner, List<CornerConnection>> rotatedBlock1 = RotateCorners(item1.block, item1.rot, level);
            //Iterating over available connections of the first block
            foreach (CornerConnection corn1 in rotatedBlock1[corner])
            {
                Dictionary<Corner, List<CornerConnection>> rotatedBlock2 = RotateCorners(item2.block, item2.rot, level);
                //Iterating over available connections of the second block
                foreach (CornerConnection corn2 in rotatedBlock2[oppositeCorner])
                {
                    if (corn1.Name == corn2.Name && IsDirectionsCompatible(corn1.Dir, corn2.Dir))
                    {
                        CartesianBlock<TQ> keyBlock1 = item1.block.ToCartesianBlock(item1.rot);
                        CartesianBlock<TQ> keyBlock2 = item2.block.ToCartesianBlock(item2.rot);

                        CartesianConstraintParameter<CartesianBlock<TQ>> r1 = parameters.TryGetOrCreate(keyBlock1);
                        r1[level][corner].Add(keyBlock2);

                        CartesianConstraintParameter<CartesianBlock<TQ>> r2 = parameters.TryGetOrCreate(keyBlock2);
                        r2[level][oppositeCorner].Add(keyBlock1);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckVerticalCompatibility<TQ>(Dictionary<CartesianBlock<TQ>, CartesianConstraintParameter<CartesianBlock<TQ>>> parameters, 
            (CartesianBlockTemplate<TQ> block, int rot) item1, (CartesianBlockTemplate<TQ> block, int rot) item2)
            where TQ : notnull
        {
            foreach (Slab slab in CartesianExtensions.Slabs)
            {
                CheckVerticalConnection(parameters, slab, item1, item2);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckVerticalConnection<TQ>(Dictionary<CartesianBlock<TQ>, CartesianConstraintParameter<CartesianBlock<TQ>>> parameters, Slab slab,
            (CartesianBlockTemplate<TQ> block, int rot) item1, (CartesianBlockTemplate<TQ> block, int rot) item2)
            where TQ : notnull
        {
            int rotationDifference = item1.rot - item2.rot;

            Slab oppositeSlab = slab.GetOpposite();
            //Iterating over available connections of the first block
            foreach (VerticalConnection vConn1 in item1.block.VerticalPools[slab])
            {
                //Iterating over available connections of the second block
                foreach (VerticalConnection vConn2 in item2.block.VerticalPools[oppositeSlab])
                {
                    if (vConn1.Name == vConn2.Name 
                        && IsDirectionsCompatible(vConn1.Dir, vConn2.Dir) 
                        && IsRotationsCompatible(vConn1.Rotations, vConn2.Rotations, rotationDifference)
                        && IsRotationsCompatible(vConn2.Rotations, vConn1.Rotations, -rotationDifference))
                    {
                        CartesianBlock<TQ> keyBlock1 = item1.block.ToCartesianBlock(item1.rot);
                        CartesianBlock<TQ> keyBlock2 = item2.block.ToCartesianBlock(item2.rot);

                        CartesianConstraintParameter<CartesianBlock<TQ>> r1 = parameters.TryGetOrCreate(item1.block.ToCartesianBlock(item1.rot));
                        r1[slab].Add(keyBlock2);

                        CartesianConstraintParameter<CartesianBlock<TQ>> r2 = parameters.TryGetOrCreate(keyBlock2);
                        r2[oppositeSlab].Add(keyBlock1);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Dictionary<Side, List<SideConnection>> RotateSides<TQ>(CartesianBlockTemplate<TQ> b, int rot, Level level)
            where TQ : notnull
        {
            var d = new Dictionary<Side, List<SideConnection>>(4);
            foreach (Side s in CartesianExtensions.Sides)
                d[s] = new List<SideConnection>(b.Levels[level].Pools[s.Rotate(4 - rot)]);   
                                                                           
                                                                           
            return d;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Dictionary<Corner, List<CornerConnection>> RotateCorners<TQ>(CartesianBlockTemplate<TQ> b, int rot, Level level)
            where TQ : notnull
        {
            var d = new Dictionary<Corner, List<CornerConnection>>(4);
            foreach (Corner c in CartesianExtensions.Corners)
                d[c] = new List<CornerConnection>(b.Levels[level].CornerPools[c.Rotate(4 - rot)]);


            return d;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsDirectionsCompatible(ConnectionDirection d1, ConnectionDirection d2) => d1 == ConnectionDirection.Both || d2 == ConnectionDirection.Both || d1 != d2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsRotationsCompatible(Rotations top, Rotations bottom, int rotationDifference)
        {
            foreach (var i in top)
            {
                foreach (var j in bottom)
                {
                    if (i - j == rotationDifference)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    public static class CartesianExtensions
    {
        public static List<Level> Levels { get; } = QSL.Helpers.ToList<Level>();

        public static List<Side> Sides { get; } = QSL.Helpers.ToList<Side>();
        public static Side Rotate(this Side original, int rotation) => (Side)(((int)original + rotation) % 4);

        public static List<Corner> Corners { get; } = QSL.Helpers.ToList<Corner>();
        public static Corner Rotate(this Corner original, int rotation) => (Corner)(((int)original + rotation) % 4);

        public static List<Slab> Slabs { get; } = QSL.Helpers.ToList<Slab>();
        public static Slab GetOpposite(this Slab original) => original == Slab.Bottom ? Slab.Top : Slab.Bottom;
    }
}
