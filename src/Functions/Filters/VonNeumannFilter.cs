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
    public class VonNeumannParameter<T>
    {
        public QVariable<T>? Left { get; set; }
        public QVariable<T>? Right { get; set; }
        public QVariable<T>? Front { get; set; }
        public QVariable<T>? Back { get; set; }
        public QVariable<T>? Top { get; set; }
        public QVariable<T>? Bottom { get; set; }

        public QVariable<T>[] ToArray()
        {
            List<QVariable<T>> neighbors = new List<QVariable<T>>();
            if (Left != null) neighbors.Add(Left);
            if (Right != null) neighbors.Add(Right);
            if (Front != null) neighbors.Add(Front);
            if (Back != null) neighbors.Add(Back);
            if (Top != null) neighbors.Add(Top);
            if (Bottom != null) neighbors.Add(Bottom);
            return neighbors.ToArray();
        }
    }

    public class VonNeumannFilter<T> : IChain<QVariable<T>, VonNeumannParameter<T>>, IChain<QVariable<T>, QVariable<T>[]>
    {
        public VonNeumannParameter<T> ApplyTo(QVariable<T> input)
        {
            var layer = EuclideanLayer<T>.With(input);
            ExceptionHelper.ThrowIfFieldIsNull(layer, nameof(layer));
            ExceptionHelper.ThrowIfFieldIsNull(layer.Machine, nameof(layer.Machine));

            var machine = layer.Machine;
            var stateLayer = EuclideanStateLayer<T>.With(machine.State);

            VonNeumannParameter<T> result = new VonNeumannParameter<T>
            {
                Left = stateLayer[(layer.X - 1, layer.Y, layer.Z)],
                Right = stateLayer[(layer.X + 1, layer.Y, layer.Z)],
                Front = stateLayer[(layer.X, layer.Y - 1, layer.Z)],
                Back = stateLayer[(layer.X, layer.Y + 1, layer.Z)],
                Top = stateLayer[(layer.X, layer.Y, layer.Z + 1)],
                Bottom = stateLayer[(layer.X, layer.Y, layer.Z - 1)]
            };

            return result;
        }

        QVariable<T>[] IChain<QVariable<T>, QVariable<T>[]>.ApplyTo(QVariable<T> input)
        {
            return ApplyTo(input).ToArray();
        }
    }
}
