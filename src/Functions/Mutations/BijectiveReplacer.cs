using System.Collections.Generic;
using qon.Exceptions;
using qon.Functions.Searchers;
using qon.Machines;
using qon.Variables;

namespace qon.Functions.Mutations
{
    public class BijectiveReplacer<TQ> : IMutationFunction<TQ> where TQ : notnull
    {
        private readonly ISearcher<TQ> _searcher;
        private readonly IMutator<TQ> _mutator;
        private int _size;

        public BijectiveReplacer(ISearcher<TQ> searcher, IMutator<TQ> mutator)
        {
            _searcher = searcher;
            _mutator = mutator;

            if (searcher.SearchDepth != mutator.MutationCount)
            {
                throw new ValidationException("Searcher count must be equal to mutator count for bijective replacer.");
            }

            _size = searcher.SearchDepth;
        }

        public List<Field<TQ>> ApplyTo(Field<TQ> input)
        {
            List<List<QVariable<TQ>>> sequences = _searcher.Search(input);
            List<Field<TQ>> samples = new List<Field<TQ>>();

            foreach (List<QVariable<TQ>> sequence in sequences)
            {
                Field<TQ> newField = input.ShallowCopy();

                List<QVariable<TQ>> localSequence = new List<QVariable<TQ>>();

                foreach (var mutationCandidate in sequence)
                {
                    QVariable<TQ> copy = mutationCandidate.Copy();
                    newField[copy.Id] = copy;
                    localSequence.Add(copy);
                }

                _mutator.Mutate(localSequence);

                samples.Add(newField);
            }

            return samples;
        }
    }
}
