using System;
using qon.Functions;
using qon.Helpers;
using qon.Layers;
using qon.Variables;
using static qon.Helpers.Helper;

namespace qon.Functions.Filters
{
    public class QPredicate<TQ> : IChain<QVariable<TQ>, bool> where TQ : notnull
    {
        public static readonly QPredicate<TQ> Empty = new QPredicate<TQ>(o => false);

        public Func<QVariable<TQ>, bool> PredicateFunction { get; protected set; }

        public QPredicate(Func<QVariable<TQ>, bool> predicateFunction)
        {
            PredicateFunction = predicateFunction;
        }

        public bool ApplyTo(QVariable<TQ> input)
        {
            return PredicateFunction(input);
        }

        public IChain<QVariable<TQ>, bool> AsIChain()
        {
            return this;
        }

        public static QPredicate<TQ> operator |(QPredicate<TQ> left, QPredicate<TQ> right)
        {
            return left.Or(right);
        }

        public static QPredicate<TQ> operator &(QPredicate<TQ> left, QPredicate<TQ> right)
        {
            return left.And(right);
        }

        public static QPredicate<TQ> Create<TLayer>(Func<TLayer, bool> predicate) where TLayer : ILayer<TQ, QVariable<TQ>>
        {
            return new QPredicate<TQ>(variable => predicate((TLayer)variable.LayerManager.GetLayer<TLayer>()));
        }

        public static implicit operator QPredicate<TQ>(TQ value)
        {
            return new QPredicate<TQ>(variable => variable.Value.CheckValue(value));
        }

        public static implicit operator QPredicate<TQ>(Optional<TQ> value)
        {
            return new QPredicate<TQ>(variable => variable.Value == value);
        }
    }
}
