using qon.Functions;
using qon.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qon.Layers.StateLayers
{
    public interface IStateLayer<T>
    {
        public Result Prepare(QVariable<T>[] field);

        public bool Validate(QVariable<T>[] field);

        public PreValidationResult PreValidate(QVariable<T>[] field);

        public void Execute(QVariable<T>[]? previousField, QVariable<T>[] currentField, Random random);
    }
}
