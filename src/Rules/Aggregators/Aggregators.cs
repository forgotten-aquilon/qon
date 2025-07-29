namespace qon.Rules.Aggregators
{
    public static class Aggregators
    {
        public static GroupingAggregator<T> GroupByTag<T>(string s)
        {
            return new GroupingAggregator<T>(v => v.Properties[s]);
        }

        public static SelectingAggregator<T> SelectByTagValue<T>(string s, object value)
        {
            return new SelectingAggregator<T>(v => v.Properties[s].Equals(value));
        }

        public static SelectingAggregator<T> All<T>()
        {
            return new SelectingAggregator<T>(v => true);
        }

        public static SelectingAggregator<T> Unassigned<T>()
        {
            return new SelectingAggregator<T>(v => v.State == SuperpositionState.Uncertain);
        }

        public static SelectingAggregator<T> Assigned<T>()
        {
            return new SelectingAggregator<T>(v => v.State != SuperpositionState.Uncertain);
        }

        public static SelectingAggregator<T> DomainContains<T>(T value)
        {
            return new SelectingAggregator<T>(v => v.Domain.ContainsValue(value));
        }

        public static SelectingAggregator<T> EqualsToValue<T>(T value)
        {
            return new SelectingAggregator<T>(v =>
                v.State != SuperpositionState.Uncertain && v.Value.CheckValue(value));
        }
    }
}
