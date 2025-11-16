using System;
using qon.Exceptions;
using qon.Layers.VariableLayers;
using qon.Variables;

namespace qon.Functions.Filters
{
    public static class EuclideanFilters
    {

        //TODO: Rewrite with EuclideanLayer<T>
        public static Func<QVariable<TQ>, QPredicate<TQ>> SelectRegion<TQ>((int x, int y, int z) l1, (int x, int y, int z) l2) where TQ : notnull
        {
            return referenceVar =>
            {
                ExceptionHelper.ThrowIfArgumentIsNull(referenceVar, nameof(referenceVar));

                return new QPredicate<TQ>(candidateVar =>
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

        public static Filter<TQ> GroupByRectangle<TQ>(int width, int height) where TQ : notnull
        {
            return Filters.GroupWith<EuclideanLayer<TQ>, TQ>(l => $"{l.X/width}x{l.Y/height}");
        }

        public static Filter<TQ> GroupByX<TQ>() where TQ : notnull
        {
            return Filters.GroupWith<EuclideanLayer<TQ>, TQ>(l => l.X);
        }

        public static Filter<TQ> GroupByY<TQ>() where TQ : notnull
        {
            return Filters.GroupWith<EuclideanLayer<TQ>, TQ>(l => l.Y);
        }

        public static Filter<TQ> GroupByZ<TQ>() where TQ : notnull
        {
            return Filters.GroupWith<EuclideanLayer<TQ>, TQ>(l => l.Z);
        }
    }
}
