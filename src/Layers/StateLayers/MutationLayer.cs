using qon.Exceptions;
using qon.Functions;
using qon.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace qon.Layers.StateLayers
{
    public class MutationLayerParameter<T>
    {
        public Func<Field<T>, List<Field<T>>>? MutationFunction { get; set; }

        public Func<Field<T>, int>? Fitness;
    }

    public class MutationLayer<T> : BaseLayer<T, MutationLayer<T>, MachineState<T>>, ILayer<T, MachineState<T>>, IStateLayer<T>
    {
        private Field<T>? _bestSample;

        public MutationLayerParameter<T> _parameter;

        public List<Field<T>> Samples { get; set; } = new();

        public MutationLayer()
        {
            _parameter = new MutationLayerParameter<T>();
        }

        #region Solving lifecycle

        public Result Prepare(Field<T> field)
        {
            var mutationFunction = ExceptionHelper.ThrowIfFieldIsNull(_parameter?.MutationFunction, nameof(_parameter.MutationFunction));

            Samples = mutationFunction(field);

            return Result.Success(0);
        }

        public PreValidationResult PreValidate(Field<T> field)
        {
            if (Samples.Count == 0)
            {
                return PreValidationResult.PreValidated;
            }

            int fitness = int.MaxValue;
            Field<T> bestSample = new Field<T>(Machine);
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
                //TODO: Update with proper copy of other properties
                field[i].Value = bestSample[i].Value;
            }

            _bestSample = bestSample;

            if (fitness == 0)
            {
                return PreValidationResult.PreValidated;
            }

            return PreValidationResult.NotValidated;
        }

        public bool Validate(Field<T> field)
        {
            return true;
        }

        public void Execute(Field<T>? previousField, Field<T> currentField, Random random)
        {
            ExceptionHelper.ThrowIfInternalValueIsNull(_bestSample);
            currentField.Update(_bestSample.Variables);
        }

        #endregion

        public override ILayer<T, MachineState<T>> Copy()
        {
            throw new NotImplementedException();
        }
    }
}
