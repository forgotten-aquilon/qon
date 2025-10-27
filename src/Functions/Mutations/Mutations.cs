using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Helpers;
using qon.Layers.VariableLayers;

namespace qon.Functions.Mutations
{
    public static class Mutations<T>
    {
        public static VariableMutation<T> RandomFromDomain = new VariableMutation<T>(v =>
        {
            v.Value = Optional<T>.Of(DomainLayer<T>.With(v).GetRandomValue(v.Machine.Random));
        });
    }
}
    