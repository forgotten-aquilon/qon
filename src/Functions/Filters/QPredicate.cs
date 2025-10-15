using System;
using qon.Functions;
using qon.Helpers;
using qon.Variables;
using qon.Variables.Layers;
using static qon.Helpers.Helper;

namespace qon.Functions.Filters
{
    public class QPredicate<T> : IChain<SuperpositionVariable<T>, bool>
    {
        public static readonly QPredicate<T> Empty = new QPredicate<T>(o => false);

        public Func<SuperpositionVariable<T>, bool> PredicateFunction { get; protected set; }

        public QPredicate(Func<SuperpositionVariable<T>, bool> predicateFunction)
        {
            PredicateFunction = predicateFunction;
        }

        public bool ApplyTo(SuperpositionVariable<T> input)
        {
            return PredicateFunction(input);
        }

        public IChain<SuperpositionVariable<T>, bool> AsIChain()
        {
            return this;
        }

        public static QPredicate<T> operator |(QPredicate<T> left, QPredicate<T> right)
        {
            return left.Or(right);
        }

        public static QPredicate<T> operator &(QPredicate<T> left, QPredicate<T> right)
        {
            return left.And(right);
        }

        public static QPredicate<T> Create<TVariable>(Func<TVariable, bool> predicate) where TVariable : SuperpositionVariable<T>
        {
            return new QPredicate<T>(PredicateBuilder.For<T,TVariable>(predicate));
        }

        public static QPredicate<T> Create1<TLayer>(Func<TLayer, bool> predicate) where TLayer : ILayer<T>
        {
            return new QPredicate<T>(variable => predicate((TLayer)variable.Layers.GetLayer<TLayer>()));
        }
    }
}