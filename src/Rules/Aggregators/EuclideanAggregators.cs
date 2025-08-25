using qon.Variables;
using System;

namespace qon.Rules.Aggregators
{
    public static class EuclideanAggregators
    {
        public static Func<SuperpositionVariable<T>, SelectingAggregator<T>> SelectRegion<T>((int x, int y, int z) l1, (int x, int y, int z) l2)
        {
            return referenceVar =>
            {
                if (referenceVar == null)
                    throw new ArgumentNullException(nameof(referenceVar));

                return new SelectingAggregator<T>(candidateVar =>
                {
                    int rx = (int)referenceVar.Properties["x"];
                    int ry = (int)referenceVar.Properties["y"];
                    int rz = (int)referenceVar.Properties["z"];

                    int cx = (int)candidateVar.Properties["x"];
                    int cy = (int)candidateVar.Properties["y"];
                    int cz = (int)candidateVar.Properties["z"];

                    return cx >= rx - l1.x && cx <= rx + l2.x &&
                           cy >= ry - l1.y && cy <= ry + l2.y &&
                           cz >= rz - l1.z && cz <= rz + l2.z;
                });
            };
        }

        public static GroupingAggregator<T> GroupByRectangle<T>(int width, int height)
        {
            return new GroupingAggregator<T>(
                v => $"{(int)v.Properties["x"] / width}x{(int)v.Properties["y"] / height}"
            );
        }
    }
}