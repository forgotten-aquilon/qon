using qon.Functions.Propagators;
using qon.Machines;
using qon.Variables;

namespace qon.Functions
{
    public interface IPreparation<TQ> where TQ : notnull
    {
        public Result Execute(Field<TQ> field, QMachine<TQ>? machine = null);
    }
}
