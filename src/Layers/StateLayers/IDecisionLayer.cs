using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Variables;

namespace qon.Layers.StateLayers
{
    public interface IDecisionLayer<T>
    {
        public void MakeDecision(QVariable<T>[]? previousField, QVariable<T>[] currentField, Random random);
    }
}
