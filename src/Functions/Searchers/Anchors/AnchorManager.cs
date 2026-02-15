using System.Collections.Generic;
using System.Linq;
using qon.Machines;
using qon.Variables;

namespace qon.Functions.Searchers.Anchors
{
    public class AnchorManager<TQ> : ISearcher<TQ> where TQ : notnull
    {
        public List<IAnchor<TQ>> Anchors { get; private set; } = new List<IAnchor<TQ>>();
        public List<AnchoredPath<TQ>> Paths { get; private set; } = new List<AnchoredPath<TQ>>();

        public AnchorManager()
        {

        }

        public AnchorManager(List<IAnchor<TQ>> anchors)
        {
            Anchors = anchors;
        }

        public void Execute(QVariable<TQ>[] field)
        {
            Paths.Clear();
            Paths.Add(new AnchoredPath<TQ>(field));

            foreach (var anchor in Anchors)
            {
                List<AnchoredPath<TQ>> newPaths = new List<AnchoredPath<TQ>>();

                foreach (var path in Paths)
                {
                    path.Reduce(anchor.GetPredicate());
                    var additionalPaths = path.Normalize(field);

                    foreach (var additionalPath in additionalPaths)
                    {       
                        additionalPath.Update(anchor.GetAnchoredVariables(field, additionalPath.FixedPath[^1]));
                    }

                    newPaths.AddRange(additionalPaths);
                }

                Paths = new List<AnchoredPath<TQ>>(newPaths);
            }
        }

        public int SearchDepth => Anchors.Count;

        public List<List<QVariable<TQ>>> Search(Field<TQ> field)
        {
            Execute(field.Variables);

            return Paths.Select(p => p.FixedPath).ToList();
        }
    }
}
