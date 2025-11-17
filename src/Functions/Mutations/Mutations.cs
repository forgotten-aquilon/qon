using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Helpers;
using qon.Layers.VariableLayers;

namespace qon.Functions.Mutations
{
    public static class Mutations<TQ> where TQ : notnull
    {
        public static VariableMutation<TQ> RandomFromDomain = new(v =>
        {
            v.Value = Optional<TQ>.Of(DomainLayer<TQ>.With(v).GetRandomValue(v.Machine.Random));
        });

        public static VariableMutation<TQ> ToValue(TQ value)
        {
            return new VariableMutation<TQ>(v =>
            {
                v.Value = Optional<TQ>.Of(value);
            });
        }
    }
}
    