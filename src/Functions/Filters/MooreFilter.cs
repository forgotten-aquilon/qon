using qon.Exceptions;
using qon.Functions.Anchors;
using qon.Layers.StateLayers;
using qon.Layers.VariableLayers;
using qon.Variables;
using System.Collections.Generic;

namespace qon.Functions.Filters
{
    public class MooreParameter<T>
    {
        public QVariable<T>? Left { get; set; }
        public QVariable<T>? Right { get; set; }
        public QVariable<T>? Front { get; set; }
        public QVariable<T>? Back { get; set; }
        public QVariable<T>? Top { get; set; }
        public QVariable<T>? Bottom { get; set; }
        public QVariable<T>? FrontLeft { get; set; }
        public QVariable<T>? FrontRight { get; set; }
        public QVariable<T>? BackLeft { get; set; }
        public QVariable<T>? BackRight { get; set; }
        public QVariable<T>? TopLeft { get; set; }
        public QVariable<T>? TopRight { get; set; }
        public QVariable<T>? TopFront { get; set; }
        public QVariable<T>? TopBack { get; set; }
        public QVariable<T>? TopFrontLeft { get; set; }
        public QVariable<T>? TopFrontRight { get; set; }
        public QVariable<T>? TopBackLeft { get; set; }
        public QVariable<T>? TopBackRight { get; set; }
        public QVariable<T>? BottomLeft { get; set; }
        public QVariable<T>? BottomRight { get; set; }
        public QVariable<T>? BottomFront { get; set; }
        public QVariable<T>? BottomBack { get; set; }
        public QVariable<T>? BottomFrontLeft { get; set; }
        public QVariable<T>? BottomFrontRight { get; set; }
        public QVariable<T>? BottomBackLeft { get; set; }
        public QVariable<T>? BottomBackRight { get; set; }

        public QVariable<T>[] ToArray()
        {
            List<QVariable<T>> neighbors = new();

            void Add(QVariable<T>? neighbor)
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

    public class MooreFilter<T> : IChain<QVariable<T>, MooreParameter<T>>, IChain<QVariable<T>, QVariable<T>[]>
    {
        public MooreParameter<T> ApplyTo(QVariable<T> input)
        {
            var layer = EuclideanLayer<T>.With(input);
            ExceptionHelper.ThrowIfFieldIsNull(layer, nameof(layer));
            ExceptionHelper.ThrowIfFieldIsNull(layer.Machine, nameof(layer.Machine));

            var machine = layer.Machine;
            var stateLayer = EuclideanStateLayer<T>.With(machine.State);

            QVariable<T>? GetNeighbor(int dx, int dy, int dz)
            {
                return stateLayer[(layer.X + dx, layer.Y + dy, layer.Z + dz)];
            }

            MooreParameter<T> result = new()
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

        QVariable<T>[] IChain<QVariable<T>, QVariable<T>[]>.ApplyTo(QVariable<T> input)
        {
            return ApplyTo(input).ToArray();
        }
    }
}
