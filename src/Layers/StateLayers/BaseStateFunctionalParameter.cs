using System;
using qon.Functions;
using qon.Machines;
using qon.Solvers;

namespace qon.Layers.StateLayers
{
    public class BaseStateFunctionalParameter<TQ> where TQ : notnull
    {
        public Func<Field<TQ>, Result>? Preparation { get; set; }

        public Func<Field<TQ>, bool>? Validation { get; set; }

        public Func<Field<TQ>, PreValidationResult>? PreValidation { get; set; }

        public Action<Field<TQ>, Field<TQ>>? Execution { get; set; }
    }
}