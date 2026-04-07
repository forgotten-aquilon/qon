using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using qon.Helpers;
using qon.Machines;

namespace qon.QSL
{
    public static class FieldExtensions
    {
        public static Optional<TQ>[] ToValueArray<TQ>(this Field<TQ> field) where TQ : notnull
        {
            return field.Select(x => x.Value).ToArray();
        }
    }
}
