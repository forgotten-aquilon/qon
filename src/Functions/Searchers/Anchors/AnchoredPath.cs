using System.Collections.Generic;
using System.Linq;
using qon.Exceptions;
using qon.Functions.Filters;
using qon.Variables;

namespace qon.Functions.Searchers.Anchors
{
    public class AnchoredPath<TQ> where TQ : notnull
    {
        public List<QObject<TQ>> FixedPath { get; set; } = new List<QObject<TQ>>();
        public List<QObject<TQ>> AlternativeEnds { get; set; } = new List<QObject<TQ>>();

        private AnchoredPath(List<QObject<TQ>> fixedPath, QObject<TQ> end)
        {
            FixedPath = new List<QObject<TQ>>(fixedPath);
            FixedPath.Add(end);
        }

        public AnchoredPath(QObject<TQ> initialValue)
        {
            FixedPath.Add(initialValue);
        }

        public AnchoredPath(QObject<TQ>[] list)
        {
            AlternativeEnds = list.ToList();
        }

        public void Reduce(QPredicate<TQ> predicate)
        {
            AlternativeEnds = AlternativeEnds.Where(predicate.ApplyTo).ToList();
        }

        public void Update(QObject<TQ>[] ends)
        {
            if (!AlternativeEnds.Any())
            {
                throw new InternalLogicException("");
            }

            AlternativeEnds = ends.Where(e => !FixedPath.Contains(e)).ToList();
        }

        public List<AnchoredPath<TQ>> Normalize(QObject<TQ>[] field)
        {
            List<AnchoredPath<TQ>> result = new List<AnchoredPath<TQ>>();

            foreach (var end in AlternativeEnds)
            {
                var p = new AnchoredPath<TQ>(FixedPath, end);
                p.AlternativeEnds = field.Where(v => !p.FixedPath.Contains(v)).ToList();
                result.Add(p);
            }

            AlternativeEnds.Clear();
            return result;
        }
    }
}
