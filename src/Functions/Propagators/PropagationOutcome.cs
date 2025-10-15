namespace qon.Functions.Propagators
{
    public readonly struct PropagationOutcome
    {
        private readonly bool _failed;

        private PropagationOutcome(bool failed)
        {
            _failed = failed;
        }

        public static PropagationOutcome Conflict => new(true);

        public static PropagationOutcome Converged => new(false);

        public static implicit operator bool(PropagationOutcome outcome) => outcome._failed;

        public static implicit operator PropagationOutcome(bool value) => new(value);

        public override string ToString()
        {
            return _failed ? nameof(Conflict) : nameof(Converged);
        }
    }
}
