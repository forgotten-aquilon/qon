using qon.Functions.Filters;
using qon.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Functions.Mutations;

namespace qon.Functions.Anchors
{
    public static class Anchors
    {
        public static Anchor<T> VNA<T>(QPredicate<T> predicate)
        {
            return new Anchor<T>(predicate, (f, a) =>
            {
                IChain<QVariable<T>, QVariable<T>[]> filter = new VonNeumannFilter<T>() as IChain<QVariable<T>, QVariable<T>[]>;
                return filter.ApplyTo(a);
            });
        }
    }
}
