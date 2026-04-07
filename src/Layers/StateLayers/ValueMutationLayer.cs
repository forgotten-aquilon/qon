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
using qon.Helpers;
using qon.QSL;

namespace qon.Layers.StateLayers
{
    public class ValueMutationLayer<TQ> : BaseLayer<TQ, ValueMutationLayer<TQ>, MachineState<TQ>>, ILayer<TQ, MachineState<TQ>>, IStateLayer<TQ> where TQ : notnull
    {
        private Optional<TQ>[]? _bestSample;

        //TODO: Add collision protection
        private Dictionary<Optional<TQ>[], HashSet<Optional<TQ>[]>> _sampleHistory = new();

        public MutationLayerParameter<TQ> Parameter = new();

        public List<(Optional<TQ>[] values, int fitness)> Samples { get; set; } = new();

        public ValueMutationLayer() {}

        public ValueMutationLayer(MutationLayerParameter<TQ> parameter)
        {
            Parameter = parameter;
        }

        #region Solving lifecycle

        public Result Prepare(Field<TQ> field)
        {
            var mutationFunction = ExceptionHelper.ThrowIfFieldIsNull(Parameter.MutationFunction, nameof(Parameter.MutationFunction));
            ExceptionHelper.ThrowIfInternalValueIsNull(Machine);
            ExceptionHelper.ThrowIfInternalValueIsNull(Parameter.Fitness);

            Samples = mutationFunction.ApplyTo(field).Select(f => (f.ToValueArray(), Parameter.Fitness(f))).ToList();

            if (Machine.Solver.BackTrackingEnabled && _sampleHistory.TryGetValue(field.ToValueArray(), out var usedSamples))
            {
                Samples.RemoveAll(f => usedSamples.Contains(f.values));

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
            Optional<TQ>[]? bestSample = null;

            foreach (var sample in Samples)
            {
                var localFitness = sample.fitness;

                if (localFitness < fitness && localFitness >= 0)
                {
                    fitness = localFitness;
                    bestSample = sample.values;
                    pos = index;
                }

                index++;
            }

            _bestSample = bestSample;

            ExceptionHelper.ThrowIfInternalValueIsNull(_bestSample);

            Samples.RemoveAt(pos);
            var values = field.ToValueArray();
            if (Machine.Solver.BackTrackingEnabled)
            {
                if (_sampleHistory.TryGetValue(values, out var set))
                {
                    set.Add(_bestSample);
                }
                else
                {
                    _sampleHistory[values] = new(){_bestSample};
                }
            }

            if (fitness == 0)
            {
                field.UpdateWithValues(_bestSample);
                return PreValidationResult.PreValidated;
            }

            return PreValidationResult.NotValidated;
        }

        public void Execute(Field<TQ>? previousField, Field<TQ> currentField)
        {
            ExceptionHelper.ThrowIfInternalValueIsNull(_bestSample);
            ExceptionHelper.ThrowIfInternalValueIsNull(Machine);

            currentField.UpdateWithValues(_bestSample);
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
