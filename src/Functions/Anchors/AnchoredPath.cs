using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Exceptions;
using qon.Functions.Filters;
using qon.Variables;

namespace qon.Functions.Anchors
{
    public class AnchoredPath<TQ> where TQ : notnull
    {
        public List<QVariable<TQ>> FixedPath { get; set; } = new List<QVariable<TQ>>();
        public List<QVariable<TQ>> AlternativeEnds { get; set; } = new List<QVariable<TQ>>();

        private AnchoredPath(List<QVariable<TQ>> fixedPath, QVariable<TQ> end)
        {
            FixedPath = new List<QVariable<TQ>>(fixedPath);
            FixedPath.Add(end);
        }

        public AnchoredPath(QVariable<TQ> initialValue)
        {
            FixedPath.Add(initialValue);
        }

        public AnchoredPath(QVariable<TQ>[] list)
        {
            AlternativeEnds = list.ToList();
        }

        public void Reduce(QPredicate<TQ> predicate)
        {
            AlternativeEnds = AlternativeEnds.Where(predicate.ApplyTo).ToList();
        }

        public void Update(QVariable<TQ>[] ends)
        {
            if (!AlternativeEnds.Any())
            {
                throw new InternalLogicException("");
            }

            AlternativeEnds = ends.Where(e => !FixedPath.Contains(e)).ToList();
        }

        public List<AnchoredPath<TQ>> Normalize(QVariable<TQ>[] field)
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
