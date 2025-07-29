namespace qon.Rules.Guards
{
    public static class Guards
    {
        public static Guard<T> Equals<T>(T value)
        {
            return new Guard<T>(variable => variable.Value.CheckValue(value));
        }
    }
}
