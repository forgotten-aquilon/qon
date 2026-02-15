using qon.Exceptions;
using qon.Layers.StateLayers;
using qon.Layers.VariableLayers;
using qon.Variables;
using System.Collections.Generic;

namespace qon.Functions.Filters
{
    public class MooreParameter<TQ> where TQ : notnull
    {
        public QVariable<TQ>? LeftFrontTop { get; set; }
        public QVariable<TQ>? LeftFrontCenter { get; set; }
        public QVariable<TQ>? LeftFrontBottom { get; set; }

        public QVariable<TQ>? LeftCenterTop { get; set; }
        public QVariable<TQ>? LeftCenterCenter { get; set; }
        public QVariable<TQ>? LeftCenterBottom { get; set; }

        public QVariable<TQ>? LeftBackTop { get; set; }
        public QVariable<TQ>? LeftBackCenter { get; set; }
        public QVariable<TQ>? LeftBackBottom { get; set; }

        public QVariable<TQ>? CenterFrontTop { get; set; }
        public QVariable<TQ>? CenterFrontCenter { get; set; }
        public QVariable<TQ>? CenterFrontBottom { get; set; }

        public QVariable<TQ>? CenterCenterTop { get; set; }
        public QVariable<TQ>? CenterCenterBottom { get; set; }

        public QVariable<TQ>? CenterBackTop { get; set; }
        public QVariable<TQ>? CenterBackCenter { get; set; }
        public QVariable<TQ>? CenterBackBottom { get; set; }

        public QVariable<TQ>? RightFrontTop { get; set; }
        public QVariable<TQ>? RightFrontCenter { get; set; }
        public QVariable<TQ>? RightFrontBottom { get; set; }

        public QVariable<TQ>? RightCenterTop { get; set; }
        public QVariable<TQ>? RightCenterCenter { get; set; }
        public QVariable<TQ>? RightCenterBottom { get; set; }

        public QVariable<TQ>? RightBackTop { get; set; }
        public QVariable<TQ>? RightBackCenter { get; set; }
        public QVariable<TQ>? RightBackBottom { get; set; }

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

            //Left
            Add(LeftFrontTop);
            Add(LeftFrontCenter);
            Add(LeftFrontBottom);

            Add(LeftCenterTop);
            Add(LeftCenterCenter);
            Add(LeftCenterBottom);

            Add(LeftBackTop);
            Add(LeftBackCenter);
            Add(LeftBackBottom);

            //Center
            Add(CenterFrontTop);
            Add(CenterFrontCenter);
            Add(CenterFrontBottom);

            Add(CenterCenterTop);
            Add(CenterCenterBottom);

            Add(CenterBackTop);
            Add(CenterBackCenter);
            Add(CenterBackBottom);

            //Right
            Add(RightFrontTop);
            Add(RightFrontCenter);
            Add(RightFrontBottom);

            Add(RightCenterTop);
            Add(RightCenterCenter);
            Add(RightCenterBottom);

            Add(RightBackTop);
            Add(RightBackCenter);
            Add(RightBackBottom);

            return neighbors.ToArray();
        }
    }

    public class MooreFilter<TQ> : IChain<QVariable<TQ>, QVariable<TQ>[]> where TQ : notnull
    {
        public MooreParameter<TQ> ApplyTo(QVariable<TQ> input)
        {
            var layer = EuclideanLayer<TQ>.With(input);

            var machine = layer.Machine;

            var stateLayer = EuclideanStateLayer<TQ>.With(machine.State);

            QVariable<TQ>? GetNeighbor(int dx, int dy, int dz)
            {
                return stateLayer[(layer.X + dx, layer.Y + dy, layer.Z + dz)];
            }

            MooreParameter<TQ> result = new()
            {
                //Left
                LeftFrontTop = GetNeighbor(-1, 1, 1),
                LeftFrontCenter = GetNeighbor(-1, 1, 0),
                LeftFrontBottom = GetNeighbor(-1, 1, -1),

                LeftCenterTop = GetNeighbor(-1, 0, 1),
                LeftCenterCenter = GetNeighbor(-1, 0, 0),
                LeftCenterBottom = GetNeighbor(-1, 0, -1),

                LeftBackTop = GetNeighbor(-1, -1, 1),
                LeftBackCenter = GetNeighbor(-1, -1, 0),
                LeftBackBottom = GetNeighbor(-1, -1, -1),

                //Center
                CenterFrontTop = GetNeighbor(0, 1, 1),
                CenterFrontCenter = GetNeighbor(0, 1, 0),
                CenterFrontBottom = GetNeighbor(0, 1, -1),

                CenterCenterTop = GetNeighbor(0, 0, 1),
                CenterCenterBottom = GetNeighbor(0, 0, -1),

                CenterBackTop = GetNeighbor(0, -1, 1),
                CenterBackCenter = GetNeighbor(0, -1, 0),
                CenterBackBottom = GetNeighbor(0, -1, -1),

                //Right
                RightFrontTop = GetNeighbor(1, 1, 1),
                RightFrontCenter = GetNeighbor(1, 1, 0),
                RightFrontBottom = GetNeighbor(1, 1, -1),

                RightCenterTop = GetNeighbor(1, 0, 1),
                RightCenterCenter = GetNeighbor(1, 0, 0),
                RightCenterBottom = GetNeighbor(1, 0, -1),

                RightBackTop = GetNeighbor(1, -1, 1),
                RightBackCenter = GetNeighbor(1, -1, 0),
                RightBackBottom = GetNeighbor(1, -1, -1),
            };

            return result;
        }

        QVariable<TQ>[] IChain<QVariable<TQ>, QVariable<TQ>[]>.ApplyTo(QVariable<TQ> input)
        {
            return ApplyTo(input).ToArray();
        }
    }
}
