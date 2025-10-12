namespace qon.Functions.Propagators
{
    public struct ConstraintResult
    {
        public bool IsSuccess { get; set; }
        public int ChangesAmount { get; set; }

        public ConstraintResult(bool isSuccess, int changes)
        {
            IsSuccess = isSuccess; 
            ChangesAmount = changes;
        }
    }
}
