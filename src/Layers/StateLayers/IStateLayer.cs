using qon.Functions;
using qon.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Machines;
using qon.Solvers;

namespace qon.Layers.StateLayers
{
    public interface IStateLayer<TQ> where TQ : notnull
    {
        public Result Prepare(Field<TQ> field);

        public bool Validate(Field<TQ> field);

        public PreValidationResult PreValidate(Field<TQ> field);

        public void Execute(Field<TQ>? previousField, Field<TQ> currentField, Random random);
    }
}
