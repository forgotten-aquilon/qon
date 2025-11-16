using System;
using qon.Exceptions;
using qon.Layers.VariableLayers;
using qon.Variables;

namespace qon.Functions.Filters
{
    public static class EuclideanFilters
    {
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
