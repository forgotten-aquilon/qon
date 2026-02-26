using qon.Exceptions;
using qon.Functions;
using qon.Machines;
using qon.Solvers;
using qon.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qon.Layers.StateLayers
{
    public interface IStateLayer<TQ> where TQ : notnull
    {
        BaseStateFunctionalParameter<TQ> BaseParameter { get; set; }

        public Result Prepare(Field<TQ> field);

        public PreValidationResult PreValidate(Field<TQ> field);

        public bool Validate(Field<TQ> field);

        public void Execute(Field<TQ>? previousField, Field<TQ> currentField);
    }
}
