using qon.Exceptions;
using System;

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

        public bool TryHandleOutcome(ref int unsolvedChanges, out ConstraintResult conflictResult)
        {
            conflictResult = default;

            switch (Outcome)
            {
                case PropagationOutcome.UnderConstrained:
                    unsolvedChanges++;
                    return true;
                case PropagationOutcome.Converged:
                    return true;
                case PropagationOutcome.Conflict:
                    conflictResult = this;
                    return false;
                default:
                    throw new NonExhaustiveExpressionException(Outcome);
            }
        }

        public bool IsConflictOutcome(ref int unsolvedChanges, out ConstraintResult conflictResult)
        {
            unsolvedChanges += ChangesAmount;

            conflictResult = this;

            return Outcome == PropagationOutcome.Conflict;
        }
    }
}
