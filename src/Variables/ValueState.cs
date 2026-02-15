namespace qon.Variables
{
    /// <summary>
    /// Represents state of the variable's value
    /// </summary>
    public enum ValueState
    {
        /// <summary>
        /// Variable has a value, which was assigned before running Solution Machine
        /// </summary>
        Constant,

        /// <summary>
        /// Variable has a value, which were assigned while running Solution Machine
        /// </summary>
        Defined,

        /// <summary>
        /// Variable doesn't have a value
        /// </summary>
        Uncertain,
    }
}
