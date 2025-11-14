using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Functions.Mutations;
using qon.Variables;

namespace qon.Functions.Replacers
{
    public class BijectiveReplacer<T> : IReplacer<T>
    {
        private readonly ISearcher<T> _searcher;
        private readonly IMutator<T> _mutator;
        private int _size;

        public BijectiveReplacer(ISearcher<T> searcher, IMutator<T> mutator)
        {
            _searcher = searcher;
            _mutator = mutator;

            if (searcher.SearchDepth != mutator.MutationCount)
            {
                //TODO: Custom exception
                throw new ArgumentException("Searcher count must be equal to mutator count for bijective replacer.");
            }

            _size = searcher.SearchDepth;
        }

        public List<Field<T>> All(Field<T> field)
        {
            var sequences = _searcher.Search(field);
            List<Field<T>> samples = new();

            foreach (var sequence in sequences)
            {
                var newField = field.Copy();

                //TODO Fix order
                var localSequence = sequence.Select(v => newField[v.Id]).ToList();

                _mutator.Mutate(localSequence);

                samples.Add(newField);
            }

            return samples;
        }
    }
}
