using qon.Variables.Domains;
using System;
using System.Collections.Generic;
using System.Text;

namespace qon.QSL
{
    public static partial class Domains
    {
        public static TSelf SetWeight<TQ, TSelf>(this IWeightedDomain<TQ> domain, TQ value, int weight) where TQ: notnull where TSelf: IWeightedDomain<TQ>
        {
            domain.UpdateWeight(value, weight);

            return (TSelf)domain;
        }

        public static IWeightedDomain<TQ> SetWeight<TQ>(this IWeightedDomain<TQ> domain, TQ value, int weight) where TQ : notnull
        {
            domain.UpdateWeight(value, weight);

            return domain;
        }
    }
}
