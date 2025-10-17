using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qon.Solvers
{
    public abstract class BaseSolver<T> : IEnumerator<MachineState<T>>
    {
        protected readonly QMachine<T> _machine;

        public MachineState<T> Current => _machine.State;

        object IEnumerator.Current => Current;

        public BaseSolver(QMachine<T> machine)
        {
            _machine = machine;
        }

        public virtual void Dispose()
        {
            throw new NotImplementedException();
        }

        public virtual bool MoveNext()
        {
            throw new NotImplementedException();
        }

        public virtual void Reset()
        {
            throw new NotImplementedException();
        }   
    }
}
