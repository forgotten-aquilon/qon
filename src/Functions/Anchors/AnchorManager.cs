using qon.Functions.Filters;
using qon.Variables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qon.Functions.Anchors
{
    public class AnchorManager<T>
    {
        public List<IAnchor<T>> Anchors { get; private set; } = new List<IAnchor<T>>();
        public List<AnchoredPath<T>> Paths { get; private set; } = new List<AnchoredPath<T>>();

        public void Execute(QVariable<T>[] field)
        {
            Paths.Add(new AnchoredPath<T>(field));

            foreach (var anchor in Anchors)
            {
                List<AnchoredPath<T>> newPaths = new List<AnchoredPath<T>>();

                foreach (var path in Paths)
                {
                    path.Reduce(anchor.GetPredicate());
                    var additionalPaths = path.Normalize();

                    foreach (var additionalPath in additionalPaths)
                    {       
                        additionalPath.Update(anchor.GetAnchoredVariables(field, additionalPath.FixedPath[^1]));
                    }

                    newPaths.AddRange(additionalPaths);
                }

                Paths = new List<AnchoredPath<T>>(newPaths);
            }
        }
    }
}
