using qon.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace qon.Rules
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

    public sealed class EuclideanBlockTemplate<T>
    {
        public T Value { get; }

        public bool SelfConnection { get; }
        public Rotations Rotations { get; }
        public Dictionary<Side, List<SideConnection>> Pools { get; }
        public Dictionary<Slab, List<VerticalConnection>> VerticalPools { get; }

        public EuclideanBlockTemplate(T value, Rotations rotations, bool selfConnection = true)
        {
            Value = value;
            Pools = new Dictionary<Side, List<SideConnection>>
            {
                [Side.Front] = new(),
                [Side.Right] = new(),
                [Side.Back] = new(),
                [Side.Left] = new()
            };
            VerticalPools = new Dictionary<Slab, List<VerticalConnection>>()
            {
                [Slab.Top] = new(),
                [Slab.Bottom] = new(),
            };
            SelfConnection = selfConnection;

            Rotations = rotations;
        }

        public EuclideanBlockTemplate(T value, bool selfConnection = true) : this(value, new Rotations(), selfConnection) { }

        public EuclideanBlockTemplate<T> Add(Side side, string connName, ConnectionDirection dir = ConnectionDirection.Both)
        {
            Pools[side].Add(new SideConnection(connName, dir));
            return this;
        }

        public EuclideanBlockTemplate<T> Add(Slab slab, string connName, ConnectionDirection dir = ConnectionDirection.Both)
        {
            VerticalPools[slab].Add(new VerticalConnection(connName, dir));
            return this;
        }

        public EuclideanBlock<T> ToEuclideanBlock(int rot)
        {
            return new EuclideanBlock<T>(Value, rot);
        }
    }

    public struct EuclideanBlock<T>
    {
        public T Value { private set; get; }
        public int Rotation { private set; get; }

        public EuclideanBlock(T value, int rotation)
        {
            Value = value;
            Rotation = rotation;
        }
    }

    public static class EuclideanRotationHelper
    {
        public static Dictionary<EuclideanBlock<T>, EuclideanRuleParameter<EuclideanBlock<T>>> GenerateConnections<T>(List<EuclideanBlockTemplate<T>> blocks)
        {
            Dictionary<EuclideanBlock<T>, EuclideanRuleParameter<EuclideanBlock<T>>> parameters = new ();

            EuclideanBlockTemplate<T> block1;
            EuclideanBlockTemplate<T> block2;

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
                            CheckSideCompatibility(parameters, (block1, rot1), (block2, rot2));

                            CheckVerticalCompatibility(parameters, (block1, rot1), (block2, rot2));
                        }
                    }
                }
            }

            return parameters;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckSideCompatibility<T>(Dictionary<EuclideanBlock<T>, EuclideanRuleParameter<EuclideanBlock<T>>> parameters, 
            (EuclideanBlockTemplate<T> block, int rot) item1, (EuclideanBlockTemplate<T> block, int rot) item2)
        {
            //Iterating over opposite sides of both blocks
            //"side" is the side of the first block
            foreach (Side side in SideExtensions.Sides)
            {
                CheckSideConnection(parameters, side, item1, item2);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckSideConnection<T>(Dictionary<EuclideanBlock<T>, EuclideanRuleParameter<EuclideanBlock<T>>> parameters, Side side, 
            (EuclideanBlockTemplate<T> block, int rot) item1, (EuclideanBlockTemplate<T> block, int rot) item2)
        {
            //"oppositeSide" is the side of the second block, which can be potentially connected to the side of the first block
            Side oppositeSide = (Side)(((int)side + 2) % 4);

            var rotatedBlock1 = RotatePools(item1.block, item1.rot);
            //Iterating over available connections of the first block
            foreach (SideConnection conn1 in rotatedBlock1[side])
            {
                Dictionary<Side, List<SideConnection>> rotatedBlock2 = RotatePools(item2.block, item2.rot);
                //Iterating over avaiable connections of the second block
                foreach (SideConnection conn2 in rotatedBlock2[oppositeSide])
                {
                    if (conn1.Name == conn2.Name && IsDirectionsCompatible(conn1.Dir, conn2.Dir))
                    {
                        EuclideanBlock<T> keyBlock1 = item1.block.ToEuclideanBlock(item1.rot);
                        EuclideanBlock<T> keyBlock2 = item2.block.ToEuclideanBlock(item2.rot);

                        EuclideanRuleParameter<EuclideanBlock<T>> r1 = parameters.TryGetOrCreate(keyBlock1);
                        r1[side].Add(keyBlock2);

                        EuclideanRuleParameter<EuclideanBlock<T>> r2 = parameters.TryGetOrCreate(keyBlock2);
                        r2[oppositeSide].Add(keyBlock1);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckVerticalCompatibility<T>(Dictionary<EuclideanBlock<T>, EuclideanRuleParameter<EuclideanBlock<T>>> parameters, 
            (EuclideanBlockTemplate<T> block, int rot) item1, (EuclideanBlockTemplate<T> block, int rot) item2)
        {
            foreach (Slab slab in SideExtensions.Slabs)
            {
                CheckVerticalConnection(parameters, slab, item1, item2);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckVerticalConnection<T>(Dictionary<EuclideanBlock<T>, EuclideanRuleParameter<EuclideanBlock<T>>> parameters, Slab slab,
            (EuclideanBlockTemplate<T> block, int rot) item1, (EuclideanBlockTemplate<T> block, int rot) item2)
        {
            int rotationDifference = item1.rot - item2.rot;

            Slab oppositeSlab = slab.GetOpposite();
            //Iterating over available connections of the first block
            foreach (VerticalConnection vConn1 in item1.block.VerticalPools[slab])
            {
                //Iterating over avaiable connections of the second block
                foreach (VerticalConnection vConn2 in item2.block.VerticalPools[oppositeSlab])
                {
                    if (vConn1.Name == vConn2.Name 
                        && IsDirectionsCompatible(vConn1.Dir, vConn2.Dir) 
                        && IsRotationsCompatible(vConn1.Rotations, vConn2.Rotations, rotationDifference)
                        && IsRotationsCompatible(vConn2.Rotations, vConn1.Rotations, -rotationDifference))
                    {
                        EuclideanBlock<T> keyBlock1 = item1.block.ToEuclideanBlock(item1.rot);
                        EuclideanBlock<T> keyBlock2 = item2.block.ToEuclideanBlock(item2.rot);

                        EuclideanRuleParameter<EuclideanBlock<T>> r1 = parameters.TryGetOrCreate(item1.block.ToEuclideanBlock(item1.rot));
                        r1[slab].Add(keyBlock2);

                        EuclideanRuleParameter<EuclideanBlock<T>> r2 = parameters.TryGetOrCreate(keyBlock2);
                        r2[oppositeSlab].Add(keyBlock1);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Dictionary<Side, List<SideConnection>> RotatePools<T>(EuclideanBlockTemplate<T> b, int rot)
        {
            var d = new Dictionary<Side, List<SideConnection>>(4);
            foreach (Side s in SideExtensions.Sides)
                d[s] = new List<SideConnection>(b.Pools[s.Rotate(4 - rot)]);   
                                                                           
                                                                           
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

    public static class SideExtensions
    {
        public static List<Side> Sides { get; } = Helper.ToList<Side>();
        public static Side Rotate(this Side original, int rotation) => (Side)(((int)original + rotation) % 4);

        public static List<Slab> Slabs { get; } = Helper.ToList<Slab>();
        public static Slab GetOpposite(this Slab original) => original == Slab.Bottom ? Slab.Top : Slab.Bottom;
    }
}
