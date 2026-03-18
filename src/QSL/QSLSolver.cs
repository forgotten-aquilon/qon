using qon.Machines;
using qon.Solvers;
using System;
using System.Collections.Generic;
using System.Text;

namespace qon
{
    public static partial class QSL
    {
        public static Func<QMachine<TQ>, ISolver<TQ>> DefaultSolver<TQ>(DefaultSolver<TQ>.SolverParameter parameter) where TQ: notnull
        {
            return Solvers.DefaultSolver<TQ>.InjectWith(parameter);
        }
    }
}
