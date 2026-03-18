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
        public MutationFunction<TQ>? MutationFunction { get; set; }

        public Func<Field<TQ>, int>? Fitness { get; set; }

        public Func<Field<TQ>, bool>? Validation { get; set; }
    }

    public class MutationLayer<TQ> : BaseLayer<TQ, MutationLayer<TQ>, MachineState<TQ>>, ILayer<TQ, MachineState<TQ>>, IStateLayer<TQ> where TQ : notnull
    {
        private Field<TQ>? _bestSample;

        //TODO: Add collision protection
        private static Dictionary<Field<TQ>, HashSet<Field<TQ>>> _sampleHistory = new Dictionary<Field<TQ>, HashSet<Field<TQ>>>();

        public MutationLayerParameter<TQ> Parameter = new();

        public List<Field<TQ>> Samples { get; set; } = new();

        public MutationLayer() {}

        public MutationLayer(MutationLayerParameter<TQ> parameter)
        {
            Parameter = parameter;
        }

        #region Solving lifecycle

        public Result Prepare(Field<TQ> field)
        {
            var mutationFunction = ExceptionHelper.ThrowIfFieldIsNull(Parameter.MutationFunction, nameof(Parameter.MutationFunction));
            ExceptionHelper.ThrowIfInternalValueIsNull(Machine);

            Samples = mutationFunction.ApplyTo(field).Select(f => f.Copy()).ToList();

            if (Machine.Solver.BackTrackingEnabled && _sampleHistory.TryGetValue(field, out var usedSamples))
            {
                Samples.RemoveAll(f => usedSamples.Contains(f));

                if (Samples.Count == 0)
                {
                    return Result.HasErrors();
                }
            }

            return Result.Success(0);
        }

        public PreValidationResult PreValidate(Field<TQ> field)
        {
            if (Samples.Count == 0)
            {
                return PreValidationResult.PreValidated;
            }

            ExceptionHelper.ThrowIfInternalValueIsNull(Parameter.Fitness);
            ExceptionHelper.ThrowIfInternalValueIsNull(Machine);

            int fitness = int.MaxValue;
            int pos = 0;
            int index = 0;
            Field<TQ> bestSample = new Field<TQ>(Machine);

            foreach (var sample in Samples)
            {
                var localFitness = Parameter.Fitness(sample);
                if (localFitness < fitness && localFitness >= 0)
                {
                    fitness = localFitness;
                    bestSample = sample;
                    pos = index;
                }

                index++;
            }

            _bestSample = bestSample;

            Samples.RemoveAt(pos);

            if (Machine.Solver.BackTrackingEnabled)
            {
                if (_sampleHistory.TryGetValue(field, out var set))
                {
                    set.Add(_bestSample);
                }
                else
                {
                    _sampleHistory[field] = new HashSet<Field<TQ>>{_bestSample};
                }
            }

            if (fitness == 0)
            {
                field.Update(_bestSample.Variables);
                return PreValidationResult.PreValidated;
            }

            return PreValidationResult.NotValidated;
        }

        public void Execute(Field<TQ>? previousField, Field<TQ> currentField)
        {
            ExceptionHelper.ThrowIfInternalValueIsNull(_bestSample);
            ExceptionHelper.ThrowIfInternalValueIsNull(Machine);

            currentField.Update(_bestSample.Variables);
        }

        public bool Validate(Field<TQ> field)
        {
            if (Parameter.Validation is { } validationFunc)
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
