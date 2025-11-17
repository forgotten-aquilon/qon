using System.Collections.Generic;
using qon.Exceptions;

namespace qon.Functions.Constraints
{
    public enum Side
    {
        Front = 0,
        Right = 1,
        Back = 2,
        Left = 3
    }

    public enum Slab
    {
        Top = 0,
        Bottom = 1,
    }

    public class EuclideanConstraintParameter<TQ> where TQ : notnull
    {
        public HashSet<TQ> Left { get; set; } = new();
        public HashSet<TQ> Right { get; set; } = new();
        public HashSet<TQ> Front { get; set; } = new();
        public HashSet<TQ> Back { get; set; } = new();
        public HashSet<TQ> Top { get; set; } = new();
        public HashSet<TQ> Bottom { get; set; } = new();

        public HashSet<TQ> this[Side side] =>
            side switch
            {
                Side.Front => Front,
                Side.Right => Right,
                Side.Back => Back,
                Side.Left => Left,
                _ => throw new NonExhaustiveExpressionException(side)
            };

        public HashSet<TQ> this[Slab side] =>
            side switch
            {
                Slab.Top => Top,
                Slab.Bottom => Bottom,
                _ => throw new NonExhaustiveExpressionException(side)
            };
    }
}