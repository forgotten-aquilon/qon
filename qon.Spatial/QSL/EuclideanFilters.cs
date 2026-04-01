using qon.Functions;
using qon.Functions.Filters;
using qon.Layers.VariableLayers;
using qon.Variables;
using System;
using System.Collections.Generic;
using System.Text;

namespace qon.QSL
{
    public static class EuclideanFilters
    {
        public static Filter<TQ> GroupByRectangle<TQ>(int width, int height) where TQ : notnull
        {
            return qon.QSL.Filters.GroupWith<EuclideanLayer<TQ>, TQ>(l => $"{l.X / width}x{l.Y / height}");
        }

        public static Filter<TQ> GroupByX<TQ>() where TQ : notnull
        {
            return qon.QSL.Filters.GroupWith<EuclideanLayer<TQ>, TQ>(l => l.X);
        }

        public static Filter<TQ> GroupByY<TQ>() where TQ : notnull
        {
            return qon.QSL.Filters.GroupWith<EuclideanLayer<TQ>, TQ>(l => l.Y);
        }

        public static Filter<TQ> GroupByZ<TQ>() where TQ : notnull
        {
            return qon.QSL.Filters.GroupWith<EuclideanLayer<TQ>, TQ>(l => l.Z);
        }

        public static QPredicate<TQ> MooreFilter<TQ>(Func<QObject<TQ>[], bool> func) where TQ : notnull
        {
            var moore = new MooreFilter<TQ>() as IChain<QObject<TQ>, QObject<TQ>[]>;
            return new QPredicate<TQ>(v => func(moore.ApplyTo(v)));
        }
    }
}
