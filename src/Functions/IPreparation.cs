using qon.Functions.Propagators;
using qon.Machines;
using qon.Variables;

namespace qon.Functions
{
    public interface IPreparation<T>
    {
        public Result Execute(Field<T> field, QMachine<T>? machine = null);
    }
}
