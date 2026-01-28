namespace qon.Helpers
{
    public interface ICopy<T>
    {
        /// <summary>
        /// Returns deep copy of current instance of the object
        /// </summary>
        /// <returns></returns>
        T Copy();
    }
}
