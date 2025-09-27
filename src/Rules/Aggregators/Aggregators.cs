using qon.Exceptions;
using qon.Variables;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace qon.Rules.Aggregators
{
    public static class Aggregators
    {
        public static GroupingAggregator<T> GroupByTag<T>(string s)
        {
            return new GroupingAggregator<T>(v => v.GetNullOrValueProperty(s) 
                ?? throw new InternalLogicException($"Variable '{v.Name}' is missing required tag '{s}' for grouping."));
        }

        public static SelectingAggregator<T> SelectByTagValue<T>(string s, object value)
        {
            return new SelectingAggregator<T>(v => object.Equals(v.GetNullOrValueProperty(s), value));
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
