using qon.Exceptions;
using qon.Layers.StateLayers;
using qon.Layers.VariableLayers;
using qon.Variables;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using qon.QSL;

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
            MooreParameter<TQ> result = new()
            {
                TopFrontLeft = input.GetNeighbor(-1, 1, 1),
                TopFrontCenter = input.GetNeighbor(0, 1, 1),
                TopFrontRight = input.GetNeighbor(1, 1, 1),
                TopMedianLeft = input.GetNeighbor(-1, 0, 1),
                TopMedianCenter = input.GetNeighbor(0, 0, 1),
                TopMedianRight = input.GetNeighbor(1, 0, 1),
                TopBackLeft = input.GetNeighbor(-1, -1, 1),
                TopBackCenter = input.GetNeighbor(0, -1, 1),
                TopBackRight = input.GetNeighbor(1, -1, 1),

                MiddleFrontLeft = input.GetNeighbor(-1, 1, 0),
                MiddleFrontCenter = input.GetNeighbor(0, 1, 0),
                MiddleFrontRight = input.GetNeighbor(1, 1, 0),
                MiddleMedianLeft = input.GetNeighbor(-1, 0,0),
                MiddleMedianRight = input.GetNeighbor(1, 0, 0),
                MiddleBackLeft = input.GetNeighbor(-1, -1, 0),
                MiddleBackCenter = input.GetNeighbor(0, -1, 0),
                MiddleBackRight = input.GetNeighbor(1, -1, 0),

                BottomFrontLeft = input.GetNeighbor(-1, 1, -1),
                BottomFrontCenter = input.GetNeighbor(0, 1, -1),
                BottomFrontRight = input.GetNeighbor(1, 1, -1),
                BottomMedianLeft = input.GetNeighbor(-1, 0, -1),
                BottomMedianCenter = input.GetNeighbor(0, 0, -1),
                BottomMedianRight = input.GetNeighbor(1, 0, -1),
                BottomBackLeft = input.GetNeighbor(-1, -1, -1),
                BottomBackCenter = input.GetNeighbor(0, -1, -1),
                BottomBackRight = input.GetNeighbor(1, -1, -1),
            };

            return result;
        }

        public override QObject<TQ>[] ApplyTo(QObject<TQ> input)
        {
            return CreateParameter(input).ToArray();
        }
    }
}
