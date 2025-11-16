using qon.Exceptions;
using qon.Functions.Anchors;
using qon.Layers.StateLayers;
using qon.Layers.VariableLayers;
using qon.Variables;
using System.Collections.Generic;

namespace qon.Functions.Filters
{
    //TODO: Update naming
    public class MooreParameter<TQ> where TQ : notnull
    {
        public QVariable<TQ>? Left { get; set; }
        public QVariable<TQ>? Right { get; set; }
        public QVariable<TQ>? Front { get; set; }
        public QVariable<TQ>? Back { get; set; }
        public QVariable<TQ>? Top { get; set; }
        public QVariable<TQ>? Bottom { get; set; }
        public QVariable<TQ>? FrontLeft { get; set; }
        public QVariable<TQ>? FrontRight { get; set; }
        public QVariable<TQ>? BackLeft { get; set; }
        public QVariable<TQ>? BackRight { get; set; }
        public QVariable<TQ>? TopLeft { get; set; }
        public QVariable<TQ>? TopRight { get; set; }
        public QVariable<TQ>? TopFront { get; set; }
        public QVariable<TQ>? TopBack { get; set; }
        public QVariable<TQ>? TopFrontLeft { get; set; }
        public QVariable<TQ>? TopFrontRight { get; set; }
        public QVariable<TQ>? TopBackLeft { get; set; }
        public QVariable<TQ>? TopBackRight { get; set; }
        public QVariable<TQ>? BottomLeft { get; set; }
        public QVariable<TQ>? BottomRight { get; set; }
        public QVariable<TQ>? BottomFront { get; set; }
        public QVariable<TQ>? BottomBack { get; set; }
        public QVariable<TQ>? BottomFrontLeft { get; set; }
        public QVariable<TQ>? BottomFrontRight { get; set; }
        public QVariable<TQ>? BottomBackLeft { get; set; }
        public QVariable<TQ>? BottomBackRight { get; set; }

        public QVariable<TQ>[] ToArray()
        {
            List<QVariable<TQ>> neighbors = new();

            void Add(QVariable<TQ>? neighbor)
            {
                if (neighbor != null)
                {
                    neighbors.Add(neighbor);
                }
            }

            Add(Left);
            Add(Right);
            Add(Front);
            Add(Back);
            Add(Top);
            Add(Bottom);
            Add(FrontLeft);
            Add(FrontRight);
            Add(BackLeft);
            Add(BackRight);
            Add(TopLeft);
            Add(TopRight);
            Add(TopFront);
            Add(TopBack);
            Add(TopFrontLeft);
            Add(TopFrontRight);
            Add(TopBackLeft);
            Add(TopBackRight);
            Add(BottomLeft);
            Add(BottomRight);
            Add(BottomFront);
            Add(BottomBack);
            Add(BottomFrontLeft);
            Add(BottomFrontRight);
            Add(BottomBackLeft);
            Add(BottomBackRight);

            return neighbors.ToArray();
        }
    }

    public class MooreFilter<TQ> : IChain<QVariable<TQ>, MooreParameter<TQ>>, IChain<QVariable<TQ>, QVariable<TQ>[]> where TQ : notnull
    {
        public MooreParameter<TQ> ApplyTo(QVariable<TQ> input)
        {
            var layer = EuclideanLayer<TQ>.With(input);
            ExceptionHelper.ThrowIfFieldIsNull(layer, nameof(layer));
            ExceptionHelper.ThrowIfFieldIsNull(layer.Machine, nameof(layer.Machine));

            var machine = layer.Machine;
            var stateLayer = EuclideanStateLayer<TQ>.With(machine.State);

            QVariable<TQ>? GetNeighbor(int dx, int dy, int dz)
            {
                return stateLayer[(layer.X + dx, layer.Y + dy, layer.Z + dz)];
            }

            MooreParameter<TQ> result = new()
            {
                Left = GetNeighbor(-1, 0, 0),
                Right = GetNeighbor(1, 0, 0),
                Front = GetNeighbor(0, -1, 0),
                Back = GetNeighbor(0, 1, 0),
                Top = GetNeighbor(0, 0, 1),
                Bottom = GetNeighbor(0, 0, -1),
                FrontLeft = GetNeighbor(-1, -1, 0),
                FrontRight = GetNeighbor(1, -1, 0),
                BackLeft = GetNeighbor(-1, 1, 0),
                BackRight = GetNeighbor(1, 1, 0),
                TopLeft = GetNeighbor(-1, 0, 1),
                TopRight = GetNeighbor(1, 0, 1),
                TopFront = GetNeighbor(0, -1, 1),
                TopBack = GetNeighbor(0, 1, 1),
                TopFrontLeft = GetNeighbor(-1, -1, 1),
                TopFrontRight = GetNeighbor(1, -1, 1),
                TopBackLeft = GetNeighbor(-1, 1, 1),
                TopBackRight = GetNeighbor(1, 1, 1),
                BottomLeft = GetNeighbor(-1, 0, -1),
                BottomRight = GetNeighbor(1, 0, -1),
                BottomFront = GetNeighbor(0, -1, -1),
                BottomBack = GetNeighbor(0, 1, -1),
                BottomFrontLeft = GetNeighbor(-1, -1, -1),
                BottomFrontRight = GetNeighbor(1, -1, -1),
                BottomBackLeft = GetNeighbor(-1, 1, -1),
                BottomBackRight = GetNeighbor(1, 1, -1)
            };

            return result;
        }

        QVariable<TQ>[] IChain<QVariable<TQ>, QVariable<TQ>[]>.ApplyTo(QVariable<TQ> input)
        {
            return ApplyTo(input).ToArray();
        }
    }
}
