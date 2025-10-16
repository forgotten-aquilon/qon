using System;
using qon.Functions;
using qon.Helpers;
using qon.Variables;
using qon.Variables.Layers;
using static qon.Helpers.Helper;

namespace qon.Functions.Filters
{
    public class QPredicate<T> : IChain<QVariable<T>, bool>
    {
        public static readonly QPredicate<T> Empty = new QPredicate<T>(o => false);

        public Func<QVariable<T>, bool> PredicateFunction { get; protected set; }

        public QPredicate(Func<QVariable<T>, bool> predicateFunction)
        {
            PredicateFunction = predicateFunction;
        }

        public bool ApplyTo(QVariable<T> input)
        {
            return PredicateFunction(input);
        }

        public IChain<QVariable<T>, bool> AsIChain()
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

        public static QPredicate<T> Create<TLayer>(Func<TLayer, bool> predicate) where TLayer : ILayer<T>
        {
            return new QPredicate<T>(variable => predicate((TLayer)variable.Layers.GetLayer<TLayer>()));
        }
    }
}
