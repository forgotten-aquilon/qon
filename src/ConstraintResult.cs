using qon.Rules;

namespace qon
{
    public struct ConstraintResult
    {
        public PropagationOutcome Outcome { get; set; }
        public int ChangesAmount { get; set; }
    }
}
