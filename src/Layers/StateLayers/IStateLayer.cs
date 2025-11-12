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
        public Result Prepare(Field<T> field);

        public bool Validate(Field<T> field);

        public PreValidationResult PreValidate(Field<T> field);

        public void Execute(Field<T>? previousField, Field<T> currentField, Random random);
    }
}
