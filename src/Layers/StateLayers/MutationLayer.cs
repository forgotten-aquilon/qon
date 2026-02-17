using qon.Exceptions;
using qon.Functions;
using qon.Functions.Mutations;
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
    public class MutationLayerParameter<TQ> where TQ : notnull
    {
        public IMutationFunction<TQ>? MutationFunction { get; set; }

        public Func<Field<TQ>, int>? Fitness { get; set; }

        public Func<Field<TQ>, bool>? Validation { get; set; }
    }

    public class MutationLayer<TQ> : BaseLayer<TQ, MutationLayer<TQ>, MachineState<TQ>>, ILayer<TQ, MachineState<TQ>>, IStateLayer<TQ> where TQ : notnull
    {
        private Field<TQ>? _bestSample;

        private Dictionary<Guid, List<Field<TQ>>> _sampleHistory = new Dictionary<Guid, List<Field<TQ>>>();

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
            ExceptionHelper.ThrowIfInternalValueIsNull(Machine);

            Samples = _sampleHistory.TryGetValue(Machine.Solver.UniqueIteration, out var samples) ? samples : mutationFunction.ApplyTo(field);

            return Result.Success(0);
        }

        public PreValidationResult PreValidate(Field<TQ> field)
        {
            if (Samples.Count == 0)
            {
                return PreValidationResult.PreValidated;
            }

            ExceptionHelper.ThrowIfInternalValueIsNull(_parameter.Fitness);
            ExceptionHelper.ThrowIfInternalValueIsNull(Machine);

            int fitness = int.MaxValue;
            int pos = 0;
            int index = 0;
            Field<TQ> bestSample = new Field<TQ>(Machine);

            foreach (var sample in Samples)
            {
                var localFitness = _parameter.Fitness(sample);
                if (localFitness < fitness && localFitness >= 0)
                {
                    fitness = localFitness;
                    bestSample = sample;
                    pos = index;
                }

                index++;
            }

            for (int i = 0; i < bestSample.Count; i++)
            {
                field[i].Value = bestSample[i].Value;
                field[i].Properties = new Dictionary<string, ValueType>(bestSample[i].Properties);
            }

            _bestSample = bestSample;

            Samples.RemoveAt(pos);

            if (fitness == 0)
            {
                return PreValidationResult.PreValidated;
            }

            return PreValidationResult.NotValidated;
        }

        public void Execute(Field<TQ>? previousField, Field<TQ> currentField)
        {
            ExceptionHelper.ThrowIfInternalValueIsNull(_bestSample);
            ExceptionHelper.ThrowIfInternalValueIsNull(Machine);

            currentField.Update(_bestSample.Variables);

            _sampleHistory[Machine.Solver.UniqueIteration] = Samples;
        }

        public bool Validate(Field<TQ> field)
        {
            if (_parameter.Validation is { } validationFunc)
            {
                if (validationFunc(field))
                {
                    return true;
                }

                return false;
            }

            return true;
        }

        #endregion

        public override ILayer<TQ, MachineState<TQ>> Copy()
        {
            throw new NotImplementedException();
        }
    }
}
