using qon.Layers.VariableLayers;
using qon.Variables;
using qon.Variables.Domains;
using System;
using System.Collections.Generic;
using System.Text;

namespace qon.QSL
{
    public static class QLinkExtensions
    {
        public static QLink<TQ> WithDomain<TQ>(this QLink<TQ> link, IDomain<TQ> domain) where TQ : notnull
        {
            var d = DomainLayer<TQ>.GetOrCreate(link.Object);
            d.AssignDomain(domain);

            return link;
        }

        public static QLink<TQ> WithValue<TQ>(this QLink<TQ> link, TQ value) where TQ : notnull
        {
            link.Object.Value = value;

            return link;
        }
    }
}
