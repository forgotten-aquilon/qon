using System;
using qon.Exceptions;
using qon.Variables;

namespace qon.Constraints.Aggregators
{
    public static class EuclideanAggregators
    {
        public static Func<SuperpositionVariable<T>, SelectingAggregator<T>> SelectRegion<T>((int x, int y, int z) l1, (int x, int y, int z) l2)
        {
            return referenceVar =>
            {
                ExceptionHelper.ThrowIfArgumentIsNull(referenceVar, nameof(referenceVar));

                return new SelectingAggregator<T>(candidateVar =>
                {
                    int rx = (int)referenceVar["x"];
                    int ry = (int)referenceVar["y"];
                    int rz = (int)referenceVar["z"];

                    int cx = (int)candidateVar["x"];
                    int cy = (int)candidateVar["y"];
                    int cz = (int)candidateVar["z"];

                    return cx >= rx - l1.x && cx <= rx + l2.x &&
                           cy >= ry - l1.y && cy <= ry + l2.y &&
                           cz >= rz - l1.z && cz <= rz + l2.z;
                });
            };
        }

        public static GroupingAggregator<T> GroupByRectangle<T>(int width, int height)
        {
            return new GroupingAggregator<T>(
                v => $"{(int)v["x"] / width}x{(int)v["y"] / height}"
            );
        }
    }
}