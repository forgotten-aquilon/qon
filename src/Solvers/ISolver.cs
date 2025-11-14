using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qon.Solvers
{
    public interface ISolver<T> : IEnumerator<MachineState<T>>
    {
        public int StepCounter { get; }
        public int BackStepCounter { get; }
    }
}
