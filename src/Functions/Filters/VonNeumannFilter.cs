using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Exceptions;
using qon.Functions.Anchors;
using qon.Layers.StateLayers;
using qon.Layers.VariableLayers;
using qon.Variables;

namespace qon.Functions.Filters
{
    public class VonNeumannParameter<TQ> where TQ : notnull
    {
        public QVariable<TQ>? Left { get; set; }
        public QVariable<TQ>? Right { get; set; }
        public QVariable<TQ>? Front { get; set; }
        public QVariable<TQ>? Back { get; set; }
        public QVariable<TQ>? Top { get; set; }
        public QVariable<TQ>? Bottom { get; set; }

        public QVariable<TQ>[] ToArray()
        {
            List<QVariable<TQ>> neighbors = new List<QVariable<TQ>>();
            if (Left != null) neighbors.Add(Left);
            if (Right != null) neighbors.Add(Right);
            if (Front != null) neighbors.Add(Front);
            if (Back != null) neighbors.Add(Back);
            if (Top != null) neighbors.Add(Top);
            if (Bottom != null) neighbors.Add(Bottom);
            return neighbors.ToArray();
        }
    }

    public class VonNeumannFilter<TQ> : IChain<QVariable<TQ>, VonNeumannParameter<TQ>>, IChain<QVariable<TQ>, QVariable<TQ>[]> where TQ : notnull
    {
        public VonNeumannParameter<TQ> ApplyTo(QVariable<TQ> input)
        {
            var layer = EuclideanLayer<TQ>.With(input);
            ExceptionHelper.ThrowIfFieldIsNull(layer, nameof(layer));

            var machine = ExceptionHelper.ThrowIfFieldIsNull(input.Machine, nameof(input.Machine));
            var stateLayer = EuclideanStateLayer<TQ>.With(machine.State);

            VonNeumannParameter<TQ> result = new VonNeumannParameter<TQ>
            {
                Left = stateLayer[(layer.X - 1, layer.Y, layer.Z)],
                Right = stateLayer[(layer.X + 1, layer.Y, layer.Z)],
                Back = stateLayer[(layer.X, layer.Y - 1, layer.Z)],
                Front = stateLayer[(layer.X, layer.Y + 1, layer.Z)],
                Bottom = stateLayer[(layer.X, layer.Y, layer.Z - 1)],
                Top = stateLayer[(layer.X, layer.Y, layer.Z + 1)],
            };

            return result;
        }

        QVariable<TQ>[] IChain<QVariable<TQ>, QVariable<TQ>[]>.ApplyTo(QVariable<TQ> input)
        {
            return ApplyTo(input).ToArray();
        }
    }
}
