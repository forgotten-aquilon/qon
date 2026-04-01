using qon.Exceptions;
using qon.Layers.StateLayers;
using qon.Layers.VariableLayers;
using qon.Variables;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace qon.Functions.Filters
{
    public class MooreParameter<TQ> where TQ : notnull
    {
        public QObject<TQ>? TopFrontLeft { get; set; }
        public QObject<TQ>? TopFrontCenter { get; set; }
        public QObject<TQ>? TopFrontRight { get; set; }
        public QObject<TQ>? TopMedianLeft { get; set; }
        public QObject<TQ>? TopMedianCenter { get; set; }
        public QObject<TQ>? TopMedianRight { get; set; }
        public QObject<TQ>? TopBackLeft { get; set; }
        public QObject<TQ>? TopBackCenter { get; set; }
        public QObject<TQ>? TopBackRight { get; set; }


        public QObject<TQ>? MiddleFrontLeft { get; set; }
        public QObject<TQ>? MiddleFrontCenter { get; set; }
        public QObject<TQ>? MiddleFrontRight { get; set; }
        public QObject<TQ>? MiddleMedianLeft { get; set; }
        //public QObject<TQ>? MiddleMedianCenter { get; set; } //Does not exist, it is its own relative position
        public QObject<TQ>? MiddleMedianRight { get; set; }
        public QObject<TQ>? MiddleBackLeft { get; set; }
        public QObject<TQ>? MiddleBackCenter { get; set; }
        public QObject<TQ>? MiddleBackRight { get; set; }

        public QObject<TQ>? BottomFrontLeft { get; set; }
        public QObject<TQ>? BottomFrontCenter { get; set; }
        public QObject<TQ>? BottomFrontRight { get; set; }
        public QObject<TQ>? BottomMedianLeft { get; set; }
        public QObject<TQ>? BottomMedianCenter { get; set; }
        public QObject<TQ>? BottomMedianRight { get; set; }
        public QObject<TQ>? BottomBackLeft { get; set; }
        public QObject<TQ>? BottomBackCenter { get; set; }
        public QObject<TQ>? BottomBackRight { get; set; }

        public QObject<TQ>[] ToArray()
        {
            List<QObject<TQ>> neighbors = new();

            void Add(QObject<TQ>? neighbor)
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

    public class MooreFilter<TQ> : Chain<QObject<TQ>, QObject<TQ>[]> where TQ : notnull
    {
        public static MooreParameter<TQ> CreateParameter(QObject<TQ> input)
        {
            var layer = EuclideanLayer<TQ>.On(input);

            var machine = layer.Machine;

            var stateLayer = EuclideanStateLayer<TQ>.On(machine.State);


            //TODO: Refactor this into separate euclidean layer
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            QObject<TQ>? GetNeighbor(int dx, int dy, int dz)
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

        public override QObject<TQ>[] ApplyTo(QObject<TQ> input)
        {
            return CreateParameter(input).ToArray();
        }
    }
}
