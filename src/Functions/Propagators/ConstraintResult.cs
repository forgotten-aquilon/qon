namespace qon.Functions.Propagators
{
    public struct ConstraintResult
    {
        public PropagationOutcome Failed { get; set; }
        public int ChangesAmount { get; set; }

        public ConstraintResult(bool failed, int changes)
        {
            Failed = failed; 
            ChangesAmount = changes;
        }

        public PropagationOutcome Outcome => Failed;

        public static ConstraintResult HasErrors(int changes = 0)
        {
            return new ConstraintResult(true, changes);
        }

        public static ConstraintResult Success(int changes)
        {
            return new ConstraintResult(false, changes);
        }
    }
}
