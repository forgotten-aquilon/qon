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
    public class AnchoredPath<T>
    {
        public List<QVariable<T>> FixedPath { get; set; } = new List<QVariable<T>>();
        public List<QVariable<T>> AlternativeEnds { get; set; } = new List<QVariable<T>>();

        private AnchoredPath(List<QVariable<T>> fixedPath, QVariable<T> end)
        {
            FixedPath = new List<QVariable<T>>(fixedPath);
            FixedPath.Add(end);
        }

        public AnchoredPath(QVariable<T> initialValue)
        {
            FixedPath.Add(initialValue);
        }

        public AnchoredPath(QVariable<T>[] list)
        {
            AlternativeEnds = list.ToList();
        }

        public void Reduce(QPredicate<T> predicate)
        {
            AlternativeEnds = AlternativeEnds.Where(predicate.ApplyTo).ToList();
        }

        public void Update(QVariable<T>[] ends)
        {
            if (!AlternativeEnds.Any())
            {
                throw new InternalLogicException("");
            }

            AlternativeEnds = ends.Where(e => !FixedPath.Contains(e)).ToList();
        }

        public List<AnchoredPath<T>> Normalize(QVariable<T>[] field)
        {
            List<AnchoredPath<T>> result = new List<AnchoredPath<T>>();

            foreach (var end in AlternativeEnds)
            {
                var p = new AnchoredPath<T>(FixedPath, end);
                p.AlternativeEnds = field.Where(v => !p.FixedPath.Contains(v)).ToList();
                result.Add(p);
            }

            AlternativeEnds.Clear();
            return result;
        }
    }
}
