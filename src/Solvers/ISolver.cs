using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Machines;

namespace qon.Solvers
{
    public interface ISolver<TQ> : IEnumerator<MachineState<TQ>> where TQ : notnull
    {
        //TODO: Update with abstract static members when Unity supports it
        public static Func<QMachine<TQ>, ISolver<TQ>> Injection => throw new NotImplementedException();
        public int StepCounter { get; }
        public int BackStepCounter { get; }
    }
}
