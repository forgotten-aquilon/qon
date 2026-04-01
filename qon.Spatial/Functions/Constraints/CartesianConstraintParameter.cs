using qon.Exceptions;
using System.Collections.Generic;
using System.Drawing;

namespace qon.Functions.Constraints
{
    public enum Side
    {
        Front = 0,
        Right = 1,
        Back = 2,
        Left = 3
    }

    public enum Corner
    {
        FrontLeft = 0,
        FrontRight = 1,
        BackRight = 2,
        BackLeft = 3
    }

    public enum Level
    {
        Top,
        Middle,
        Bottom
    }

    public enum Slab
    {
        Top = 0,
        Bottom = 1,
    }

    public class LevelParameter<TQ> where TQ : notnull
    {
        public HashSet<TQ> Left { get; set; } = new();
        public HashSet<TQ> Right { get; set; } = new();
        public HashSet<TQ> Front { get; set; } = new();
        public HashSet<TQ> Back { get; set; } = new();

        public HashSet<TQ> FrontLeft { get; set; } = new();
        public HashSet<TQ> FrontRight { get; set; } = new();
        public HashSet<TQ> BackRight { get; set; } = new();
        public HashSet<TQ> BackLeft { get; set; } = new();

        public HashSet<TQ> this[Corner corner] =>
            corner switch
            {
                Corner.FrontLeft => FrontLeft,
                Corner.FrontRight => FrontRight,
                Corner.BackRight => BackRight,
                Corner.BackLeft => BackLeft,
                _ => throw new NonExhaustiveExpressionException(corner)
            };

        public HashSet<TQ> this[Side side] =>
            side switch
            {
                Side.Front => Front,
                Side.Right => Right,
                Side.Back => Back,
                Side.Left => Left,
                _ => throw new NonExhaustiveExpressionException(side)
            };
    }

    public class CartesianConstraintParameter<TQ> where TQ : notnull
    {
        public Dictionary<Level, LevelParameter<TQ>> Levels = new()
        {
            { Level.Top, new() },
            { Level.Middle, new() },
            { Level.Bottom, new() },
        };

        public HashSet<TQ> Top { get; set; } = new();
        public HashSet<TQ> Bottom { get; set; } = new();

        public LevelParameter<TQ> this[Level level] =>
            level switch
            {
                Level.Top => Levels[Level.Top],
                Level.Middle => Levels[Level.Middle],
                Level.Bottom => Levels[Level.Bottom],
                _ => throw new NonExhaustiveExpressionException(level)
            };

        public LevelParameter<TQ> TopLevel => Levels[Level.Top];
        public LevelParameter<TQ> CenterLevel => Levels[Level.Middle];
        public LevelParameter<TQ> BottomLevel => Levels[Level.Bottom];

        public HashSet<TQ> this[Slab side] =>
            side switch
            {
                Slab.Top => Top,
                Slab.Bottom => Bottom,
                _ => throw new NonExhaustiveExpressionException(side)
            };
    }
}