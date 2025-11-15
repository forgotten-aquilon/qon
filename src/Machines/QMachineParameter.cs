using System;
using System.Collections.Generic;
using qon.Functions.Constraints;
using qon.Solvers;
using qon.Variables;

namespace qon.Machines
{
    public class QMachineParameter<T>
    {
        public IEnumerable<QVariable<T>>? Field { get; set; }
        public Func<QMachine<T>, ISolver<T>> SolverInjection { get; set; } = DefaultSolver<T>.Injection;
        public Random Random { get; set; } = new Random();
    }
}
