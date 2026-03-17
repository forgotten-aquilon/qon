using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using qon.Machines;
using qon.Variables.Domains;

namespace qon.Variables
{
    public class QLink<TQ> where TQ: notnull
    {
        private readonly Func<QMachine<TQ>, QVariable<TQ>> _resolver;
        public QMachine<TQ> Machine { get; private set; }
        public QVariable<TQ> Variable => _resolver(Machine);

        public QLink(Func<QMachine<TQ>, QVariable<TQ>> resolver, QMachine<TQ> machine)
        {
            _resolver = resolver;
            Machine = machine;
        }

        public static implicit operator QVariable<TQ>(QLink<TQ> obj)
        {
            return obj.Variable;
        }

        public override string ToString()
        {
            return Variable.ToString();
        }
    }
}
