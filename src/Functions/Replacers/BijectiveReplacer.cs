using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Exceptions;
using qon.Functions.Mutations;
using qon.Machines;
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
            List<List<QVariable<TQ>>> sequences = _searcher.Search(field);
            List<Field<TQ>> samples = new();

            foreach (List<QVariable<TQ>> sequence in sequences)
            {
                QVariable<TQ>[] fieldCopy = new QVariable<TQ>[field.Count];
                Array.Copy(field.Variables, fieldCopy, field.Count);
                Field<TQ> newField = new Field<TQ>(field.Machine, fieldCopy);

                List<QVariable<TQ>> localSequence = new();

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
