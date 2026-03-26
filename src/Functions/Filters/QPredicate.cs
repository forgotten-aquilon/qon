using System;
using qon.Functions;
using qon.Helpers;
using qon.Layers;
using qon.Variables;

namespace qon.Functions.Filters
{
    public class QPredicate<TQ> : Chain<QObject<TQ>, bool> where TQ : notnull
    {
        public static readonly QPredicate<TQ> Empty = new QPredicate<TQ>(o => false);

        public Func<QObject<TQ>, bool> PredicateFunction { get; protected set; }

        public QPredicate(Func<QObject<TQ>, bool> predicateFunction)
        {
            PredicateFunction = predicateFunction;
        }

        public override bool ApplyTo(QObject<TQ> input)
        {
            return PredicateFunction(input);
        }

        public IChain<QObject<TQ>, bool> AsIChain()
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

        public static QPredicate<TQ> Create<TLayer>(Func<TLayer, bool> predicate) where TLayer : ILayer<TQ, QObject<TQ>>
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
