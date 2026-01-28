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

        public Result Prepare(Field<TQ> field)
        {
            return ExceptionHelper.ThrowIfFieldIsNull(BaseParameter.Preparation, nameof(BaseParameter.Preparation)).Invoke(field);
        }

        public PreValidationResult PreValidate(Field<TQ> field)
        {
            return ExceptionHelper.ThrowIfFieldIsNull(BaseParameter.PreValidation, nameof(BaseParameter.PreValidation)).Invoke(field);
        }

        public bool Validate(Field<TQ> field)
        {
            return ExceptionHelper.ThrowIfFieldIsNull(BaseParameter.Validation, nameof(BaseParameter.Validation)).Invoke(field);
        }

        public void Execute(Field<TQ>? previousField, Field<TQ> currentField)
        {
            ExceptionHelper.ThrowIfFieldIsNull(BaseParameter.Execution, nameof(BaseParameter.Execution)).Invoke(
                ExceptionHelper.ThrowIfArgumentIsNull(previousField, nameof(previousField)),
                currentField);
        }
    }
}
