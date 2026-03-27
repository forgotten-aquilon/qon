using System;
using System.Collections.Generic;
using qon.Functions.Constraints;
using qon.Solvers;
using qon.Variables;

namespace qon.Machines
{
    public class QMachineParameter<TQ> where TQ : notnull
    {
        public Func<QMachine<TQ>, ISolver<TQ>> SolverInit { get; set; } = DefaultSolver<TQ>.Injection;
        public Random Random { get; set; } = new Random();
    }
}
