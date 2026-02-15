using System;

namespace qon.Functions
{
    public interface IChain<TIn, TOut>
    {
        public TOut ApplyTo(TIn input);
    }

    public static class ChainExtensions
    {
        public static IChain<TIn, TNext> Then<TIn, TOut, TNext>(
            this IChain<TIn, TOut> first,
            IChain<TOut, TNext> second)
        {
            return new CombinedChain<TIn, TOut, TNext>(first, second);
        }
    }

    internal class CombinedChain<TIn, TMid, TOut> : IChain<TIn, TOut>
    {
        private readonly IChain<TIn, TMid> _first;
        private readonly IChain<TMid, TOut> _second;

        public CombinedChain(IChain<TIn, TMid> first, IChain<TMid, TOut> second)
        {
            _first = first;
            _second = second;
        }

        public TOut ApplyTo(TIn input) => _second.ApplyTo(_first.ApplyTo(input));
    }
}
