using System.Collections.Generic;
using qon.Functions;

namespace qon.Machines
{
    public class RuleHandler<TQ> where TQ : notnull
    {
        public List<IPreparation<TQ>> GeneralConstraints { get; set; } = new List<IPreparation<TQ>>();
        public List<IPreparation<TQ>>? ValidationConstraints { get; set; } = new List<IPreparation<TQ>>();
    }
}