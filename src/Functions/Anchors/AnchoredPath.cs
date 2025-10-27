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
            FixedPath = fixedPath;
            fixedPath.Add(end);
        }

        public AnchoredPath()
        {

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

        public List<AnchoredPath<T>> Normalize()
        {
            List<AnchoredPath<T>> result = new List<AnchoredPath<T>>();

            foreach (var end in AlternativeEnds)
            {
                result.Add(new AnchoredPath<T>(FixedPath, end));
            }
            AlternativeEnds.Clear();
            return result;
        }
    }
}
