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
        private readonly Func<QMachine<TQ>, QObject<TQ>> _resolver;
        public QMachine<TQ> Machine { get; private set; }
        public QObject<TQ> Object => _resolver(Machine);

        internal QLink(Func<QMachine<TQ>, QObject<TQ>> resolver, QMachine<TQ> machine)
        {
            _resolver = resolver;
            Machine = machine;
        }

        public static implicit operator QObject<TQ>(QLink<TQ> obj)
        {
            return obj.Object;
        }

        public override string ToString()
        {
            return Object.ToString();
        }
    }
}
