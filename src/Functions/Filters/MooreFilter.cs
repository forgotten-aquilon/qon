using qon.Exceptions;
using qon.Layers.StateLayers;
using qon.Layers.VariableLayers;
using qon.Variables;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace qon.Functions.Filters
{
    public class MooreParameter<TQ> where TQ : notnull
    {
        public QVariable<TQ>? TopFrontLeft { get; set; }
        public QVariable<TQ>? TopFrontCenter { get; set; }
        public QVariable<TQ>? TopFrontRight { get; set; }
        public QVariable<TQ>? TopMedianLeft { get; set; }
        public QVariable<TQ>? TopMedianCenter { get; set; }
        public QVariable<TQ>? TopMedianRight { get; set; }
        public QVariable<TQ>? TopBackLeft { get; set; }
        public QVariable<TQ>? TopBackCenter { get; set; }
        public QVariable<TQ>? TopBackRight { get; set; }


        public QVariable<TQ>? MiddleFrontLeft { get; set; }
        public QVariable<TQ>? MiddleFrontCenter { get; set; }
        public QVariable<TQ>? MiddleFrontRight { get; set; }
        public QVariable<TQ>? MiddleMedianLeft { get; set; }
        //public QVariable<TQ>? MiddleMedianCenter { get; set; } //Does not exist, it is its own relative position
        public QVariable<TQ>? MiddleMedianRight { get; set; }
        public QVariable<TQ>? MiddleBackLeft { get; set; }
        public QVariable<TQ>? MiddleBackCenter { get; set; }
        public QVariable<TQ>? MiddleBackRight { get; set; }

        public QVariable<TQ>? BottomFrontLeft { get; set; }
        public QVariable<TQ>? BottomFrontCenter { get; set; }
        public QVariable<TQ>? BottomFrontRight { get; set; }
        public QVariable<TQ>? BottomMedianLeft { get; set; }
        public QVariable<TQ>? BottomMedianCenter { get; set; }
        public QVariable<TQ>? BottomMedianRight { get; set; }
        public QVariable<TQ>? BottomBackLeft { get; set; }
        public QVariable<TQ>? BottomBackCenter { get; set; }
        public QVariable<TQ>? BottomBackRight { get; set; }

        public QVariable<TQ>[] ToArray()
        {
            List<QVariable<TQ>> neighbors = new();

            void Add(QVariable<TQ>? neighbor)
            {
                if (neighbor is not null)
                {
                    neighbors.Add(neighbor);
                }
            }

            Add(TopFrontLeft);
            Add(TopFrontCenter);
            Add(TopFrontRight);
            Add(TopMedianLeft);
            Add(TopMedianCenter);
            Add(TopMedianRight);
            Add(TopBackLeft);
            Add(TopBackCenter);
            Add(TopBackRight);

            Add(MiddleFrontLeft);
            Add(MiddleFrontCenter);
            Add(MiddleFrontRight);
            Add(MiddleMedianLeft);
            Add(MiddleMedianRight);
            Add(MiddleBackLeft);
            Add(MiddleBackCenter);
            Add(MiddleBackRight);

            Add(BottomFrontLeft);
            Add(BottomFrontCenter);
            Add(BottomFrontRight);
            Add(BottomMedianLeft);
            Add(BottomMedianCenter);
            Add(BottomMedianRight);
            Add(BottomBackLeft);
            Add(BottomBackCenter);
            Add(BottomBackRight);

            return neighbors.ToArray();
        }
    }

    public class MooreFilter<TQ> : IChain<QVariable<TQ>, QVariable<TQ>[]> where TQ : notnull
    {
        public static MooreFilter<TQ> Filter { get; } = new();

        public MooreParameter<TQ> ApplyTo(QVariable<TQ> input)
        {
            var layer = EuclideanLayer<TQ>.With(input);

            var machine = layer.Machine;

            var stateLayer = EuclideanStateLayer<TQ>.With(machine.State);


            //TODO: Refactor this into separate euclidean layer
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            QVariable<TQ>? GetNeighbor(int dx, int dy, int dz)
            {
                return stateLayer[(layer.X + dx, layer.Y + dy, layer.Z + dz)];
            }

            MooreParameter<TQ> result = new()
            {
                TopFrontLeft = GetNeighbor(-1, 1, 1),
                TopFrontCenter = GetNeighbor(0, 1, 1),
                TopFrontRight = GetNeighbor(1, 1, 1),
                TopMedianLeft = GetNeighbor(-1, 0, 1),
                TopMedianCenter = GetNeighbor(0, 0, 1),
                TopMedianRight = GetNeighbor(1, 0, 1),
                TopBackLeft = GetNeighbor(-1, -1, 1),
                TopBackCenter = GetNeighbor(0, -1, 1),
                TopBackRight = GetNeighbor(1, -1, 1),

                MiddleFrontLeft = GetNeighbor(-1, 1, 0),
                MiddleFrontCenter = GetNeighbor(0, 1, 0),
                MiddleFrontRight = GetNeighbor(1, 1, 0),
                MiddleMedianLeft = GetNeighbor(-1, 0,0),
                MiddleMedianRight = GetNeighbor(1, 0, 0),
                MiddleBackLeft = GetNeighbor(-1, -1, 0),
                MiddleBackCenter = GetNeighbor(0, -1, 0),
                MiddleBackRight = GetNeighbor(1, -1, 0),

                BottomFrontLeft = GetNeighbor(-1, 1, -1),
                BottomFrontCenter = GetNeighbor(0, 1, -1),
                BottomFrontRight = GetNeighbor(1, 1, -1),
                BottomMedianLeft = GetNeighbor(-1, 0, -1),
                BottomMedianCenter = GetNeighbor(0, 0, -1),
                BottomMedianRight = GetNeighbor(1, 0, -1),
                BottomBackLeft = GetNeighbor(-1, -1, -1),
                BottomBackCenter = GetNeighbor(0, -1, -1),
                BottomBackRight = GetNeighbor(1, -1, -1),
            };

            return result;
        }

        QVariable<TQ>[] IChain<QVariable<TQ>, QVariable<TQ>[]>.ApplyTo(QVariable<TQ> input)
        {
            return ApplyTo(input).ToArray();
        }
    }
}
