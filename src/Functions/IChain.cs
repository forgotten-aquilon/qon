namespace qon.Functions
{
    public interface IChain<TIn, TOut>
    {
        public TOut ApplyTo(TIn input);
        public IChain<TIn, TOut> AsIChain();

        public static TOut operator +(TIn left, IChain<TIn, TOut> right) => right.ApplyTo(left);
    }
}
