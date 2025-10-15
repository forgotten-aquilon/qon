using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Exceptions;
using qon.Variables;
using qon.Variables.Layers;

namespace qon.Functions.Filters
{
    public class VonNeumannParameter<T>
    {
        public SuperpositionVariable<T>? Left { get; set; }
        public SuperpositionVariable<T>? Right { get; set; }
        public SuperpositionVariable<T>? Front { get; set; }
        public SuperpositionVariable<T>? Back { get; set; }
        public SuperpositionVariable<T>? Top { get; set; }
        public SuperpositionVariable<T>? Bottom { get; set; }

    }

    public class VonNeumannFilter<T> : IChain<SuperpositionVariable<T>, VonNeumannParameter<T>>
    {
        public VonNeumannParameter<T> ApplyTo(SuperpositionVariable<T> input)
        {
            if (input.Layers.TryGetLayer<EuclideanLayer<T>>(out var layer))
            {
                VonNeumannParameter<T> result = new VonNeumannParameter<T>();
                result.Left = layer?.Machine[(layer.X - 1, layer.Y, layer.Z)];

                result.Right = layer?.Machine[(layer.X + 1, layer.Y, layer.Z)];

                result.Front = layer?.Machine[(layer.X, layer.Y - 1, layer.Z)];

                result.Back = layer?.Machine[(layer.X, layer.Y + 1, layer.Z)];

                result.Top = layer?.Machine[(layer.X, layer.Y, layer.Z + 1)];

                result.Bottom = layer?.Machine[(layer.X, layer.Z, layer.Z - 1)];

                return result;
            }

            //TODO
            throw new InternalLogicException("");
        }
    }
}
