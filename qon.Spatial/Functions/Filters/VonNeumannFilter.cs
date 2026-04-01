using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Exceptions;
using qon.Layers.StateLayers;
using qon.Layers.VariableLayers;
using qon.Variables;

namespace qon.Functions.Filters
{
    public class VonNeumannParameter<TQ> where TQ : notnull
    {
        public QObject<TQ>? Left { get; set; }
        public QObject<TQ>? Right { get; set; }
        public QObject<TQ>? Front { get; set; }
        public QObject<TQ>? Back { get; set; }
        public QObject<TQ>? Top { get; set; }
        public QObject<TQ>? Bottom { get; set; }

        public QObject<TQ>[] ToArray()
        {
            List<QObject<TQ>> neighbors = new List<QObject<TQ>>();
            if (Left is not null) neighbors.Add(Left);
            if (Right is not null) neighbors.Add(Right);
            if (Front is not null) neighbors.Add(Front);
            if (Back is not null) neighbors.Add(Back);
            if (Top is not null) neighbors.Add(Top);
            if (Bottom is not null) neighbors.Add(Bottom);
            return neighbors.ToArray();
        }
    }

    public class VonNeumannFilter<TQ> : Chain<QObject<TQ>, QObject<TQ>[]> where TQ : notnull
    {
        public static VonNeumannFilter<TQ> Filter { get; } = new();

        private VonNeumannFilter(){}

        public static VonNeumannParameter<TQ> CreateParameter(QObject<TQ> input)
        {
            var layer = CartesianLayer<TQ>.On(input);
            ExceptionHelper.ThrowIfFieldIsNull(layer, nameof(layer));

            var machine = ExceptionHelper.ThrowIfFieldIsNull(input.Machine, nameof(input.Machine));
            var stateLayer = CartesianStateLayer<TQ>.On(machine.State);

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

        public override QObject<TQ>[] ApplyTo(QObject<TQ> input)
        {
            return CreateParameter(input).ToArray();
        }
    }
}
