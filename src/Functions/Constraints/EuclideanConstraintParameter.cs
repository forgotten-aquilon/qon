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

    public class EuclideanConstraintParameter<T>
    {
        public HashSet<T> Left { get; set; } = new();
        public HashSet<T> Right { get; set; } = new();
        public HashSet<T> Front { get; set; } = new();
        public HashSet<T> Back { get; set; } = new();
        public HashSet<T> Top { get; set; } = new();
        public HashSet<T> Bottom { get; set; } = new();

        public EuclideanConstraintParameter()
        {
            Left = new();
            Right = new();
            Front = new();
            Back = new();
            Top = new();
            Bottom = new();
        }

        public HashSet<T> this[Side side]
        {
            get => side switch
            {
                Side.Front => Front,
                Side.Right => Right,
                Side.Back => Back,
                Side.Left => Left,
                _ => throw new NonExhaustiveExpressionException(side)
            };
        }

        public HashSet<T> this[Slab side]
        {
            get => side switch
            {
                Slab.Top => Top,
                Slab.Bottom => Bottom,
                _ => throw new NonExhaustiveExpressionException(side)
            };
        }
    }
}