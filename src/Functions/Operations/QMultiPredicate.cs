using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Variables;

namespace qon.Functions.Operations
{
    public class QMultiPredicate<T> : IChain<QVariable<T>[], QVariable<T>[]>
    {
        public Func<QVariable<T>[], QVariable<T>[]> MultiPredicate { get; set; }

        public QMultiPredicate(Func<QVariable<T>[], QVariable<T>[]> multiPredicate)
        {
            MultiPredicate = multiPredicate;
        }

        public QVariable<T>[] ApplyTo(QVariable<T>[] input)
        {
            return MultiPredicate(input);
        }
    }
}
