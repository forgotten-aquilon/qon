using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Exceptions;
using qon.Functions;
using qon.Variables;

namespace qon.Layers.StateLayers
{
    public class MutationLayerParameter<T>
    {
        public IPreparation<T>? Preparation;
        public Func<QVariable<T>[], int>? Fitness;
    }

    public class MutationLayer<T> : BaseLayer<T, MutationLayer<T>, MachineState<T>>, ILayer<T, MachineState<T>>, IStateLayer<T>
    {
        public MutationLayerParameter<T> _parameter;

        public List<QVariable<T>[]> Samples { get; set; } = new List<QVariable<T>[]>();

        public MutationLayer()
        {
            _parameter = new MutationLayerParameter<T>();
        }   

        public Result Prepare(QVariable<T>[] field)
        {
            return _parameter.Preparation?.Execute(field, Machine) ?? Result.Success(0);
        }

        public PreValidationResult PreValidate(QVariable<T>[] field)
        {
            int fitness = int.MaxValue;
            QVariable<T>[] bestSample = Array.Empty<QVariable<T>>();
            ExceptionHelper.ThrowIfInternalValueIsNull(_parameter?.Fitness);
            ExceptionHelper.ThrowIfInternalValueIsNull(Machine);
            foreach (var sample in Samples)
            {
                var f = _parameter.Fitness(sample);
                if (f < fitness && f >= 0)
                {
                    fitness = f;
                    bestSample = sample;
                }
            }

            for (int i = 0; i < bestSample.Length; i++)
            {
                field[i].Value = bestSample[i].Value;
            }

            if (fitness == 0)
            {
                return PreValidationResult.PreValidated;
            }

            return PreValidationResult.NotValidated;
        }

        public bool Validate(QVariable<T>[] field)
        {
            return true;
        }

        public void Execute(QVariable<T>[]? previousField, QVariable<T>[] currentField, Random random)
        {
            
        }

        public ILayer<T, MachineState<T>> Copy()
        {
            throw new NotImplementedException();
        }
    }
}
