namespace qon.Functions
{
    /// <summary>
    /// Represents result of some operation
    /// </summary>
    public struct Result
    {
        public bool Failed { get; private set; }

        /// <summary>
        /// Amount of changes made by this operation
        /// </summary>
        public int ChangesAmount { get; private set; }

        public Result(bool failed, int changes)
        {
            Failed = failed; 
            ChangesAmount = changes;
        }

        public static Result HasErrors(int changes = 0)
        {
            return new Result(true, changes);
        }

        public static Result Success(int changes)
        {
            return new Result(false, changes);
        }
    }
}
