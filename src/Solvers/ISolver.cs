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
        public Guid UniqueIteration { get; }

        //FUTURE: Update with abstract static members when Unity supports it
        /// <summary>
        /// Function used for initialization of Solver with the instance of <see cref="QMachine{TQ}"/>
        /// </summary>
        public static Func<QMachine<TQ>, ISolver<TQ>> Injection => throw new NotImplementedException();

        /// <summary>
        /// Current amount of all steps performed by Solver
        /// </summary>
        public int StepCounter { get; }

        /// <summary>
        /// Current amount of all steps performed by Solver while backtracking
        /// </summary>
        public int BackStepCounter { get; }

        public bool BackTrackingEnabled { get; }
    }
}
