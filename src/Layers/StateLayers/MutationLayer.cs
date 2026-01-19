using qon.Exceptions;
using qon.Functions;
using qon.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using qon.Machines;
using qon.Solvers;

namespace qon.Layers.StateLayers
{
    public class MutationLayerParameter<TQ> where TQ : notnull
    {
        public Func<Field<TQ>, List<Field<TQ>>>? MutationFunction { get; set; }

        public Func<Field<TQ>, int>? Fitness { get; set; }
    }

    public class MutationLayer<TQ> : BaseLayer<TQ, MutationLayer<TQ>, MachineState<TQ>>, ILayer<TQ, MachineState<TQ>>, IStateLayer<TQ> where TQ : notnull
    {
        private Field<TQ>? _bestSample;

        public MutationLayerParameter<TQ> _parameter;

        public List<Field<TQ>> Samples { get; set; } = new();

        public MutationLayer()
        {
            _parameter = new MutationLayerParameter<TQ>();
            BaseParameter = new BaseStateFunctionalParameter<TQ>();
        }

        #region Solving lifecycle

        public BaseStateFunctionalParameter<TQ> BaseParameter { get; set; }

        public Result Prepare(Field<TQ> field)
        {
            var mutationFunction = ExceptionHelper.ThrowIfFieldIsNull(_parameter.MutationFunction, nameof(_parameter.MutationFunction));

            Samples = mutationFunction(field);

            return Result.Success(0);
        }

        public PreValidationResult PreValidate(Field<TQ> field)
        {
            if (Samples.Count == 0)
            {
                return PreValidationResult.PreValidated;
            }

            int fitness = int.MaxValue;
            Field<TQ> bestSample = new Field<TQ>(Machine);
            ExceptionHelper.ThrowIfInternalValueIsNull(_parameter?.Fitness);
            ExceptionHelper.ThrowIfInternalValueIsNull(Machine);
            foreach (var sample in Samples)
            {
                var localFitness = _parameter.Fitness(sample);
                if (localFitness < fitness && localFitness >= 0)
                {
                    fitness = localFitness;
                    bestSample = sample;
                }
            }

            for (int i = 0; i < bestSample.Count; i++)
            {
                field[i].Value = bestSample[i].Value;
                field[i].Properties = new Dictionary<string, ValueType>(bestSample[i].Properties);
            }

            _bestSample = bestSample;

            if (fitness == 0)
            {
                return PreValidationResult.PreValidated;
            }

            return PreValidationResult.NotValidated;
        }

        public bool Validate(Field<TQ> field)
        {
            return true;
        }

        public void Execute(Field<TQ>? previousField, Field<TQ> currentField)
        {
            ExceptionHelper.ThrowIfInternalValueIsNull(_bestSample);
            currentField.Update(_bestSample.Variables);
        }

        #endregion

        public override ILayer<TQ, MachineState<TQ>> Copy()
        {
            throw new NotImplementedException();
        }
    }
}
