using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Exceptions;
using qon.Functions.Mutations;
using qon.Variables;

namespace qon.Functions.Replacers
{
    public class BijectiveReplacer<TQ> : IReplacer<TQ> where TQ : notnull
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

        public List<Field<TQ>> All(Field<TQ> field)
        {
            var sequences = _searcher.Search(field);
            List<Field<TQ>> samples = new();

            foreach (var sequence in sequences)
            {
                var newField = field.Copy();

                var localSequence = sequence.Select(v => newField[v.Id]).ToList();

                _mutator.Mutate(localSequence);

                samples.Add(newField);
            }

            return samples;
        }
    }
}
