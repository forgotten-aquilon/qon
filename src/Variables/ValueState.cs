namespace qon.Variables
{
    /// <summary>
    /// Represents state of the @object's value
    /// </summary>
    public enum ValueState
    {
        /// <summary>
        /// Object has a value, which was assigned before running Solution Machine
        /// </summary>
        Constant,

        /// <summary>
        /// Object has a value, which were assigned while running Solution Machine
        /// </summary>
        Defined,

        /// <summary>
        /// Object doesn't have a value
        /// </summary>
        Uncertain,
    }
}
