using qon.Machines;
using qon.Solvers;
using System;
using System.Collections.Generic;
using System.Text;

namespace qon.QSL
{
    public static partial class QSLSolver
    {
        public static Func<QMachine<TQ>, ISolver<TQ>> DefaultSolver<TQ>(DefaultSolver<TQ>.SolverParameter parameter) where TQ: notnull
        {
            return Solvers.DefaultSolver<TQ>.InjectWith(parameter);
        }
    }
}
