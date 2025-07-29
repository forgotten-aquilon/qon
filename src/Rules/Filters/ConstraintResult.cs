namespace qon.Rules.Filters
{
    public struct ConstraintResult
    {
        public PropagationOutcome Outcome { get; set; }
        public int ChangesAmount { get; set; }

        public ConstraintResult(PropagationOutcome outcome, int changes)
        {
            Outcome = outcome; 
            ChangesAmount = changes;
        }
    }
}
