using System.Collections.Generic;

namespace qon.Rules
{
    public class EuclideanRuleParameter<T>
    {
        public List<T> Left { get; set; } = new();
        public List<T> Right { get; set; } = new();
        public List<T> Front { get; set; } = new();
        public List<T> Back { get; set; } = new();
        public List<T> Top { get; set; } = new();
        public List<T> Bottom { get; set; } = new();
    }
}