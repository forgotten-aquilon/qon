using qon.Exceptions;
using qon.Functions.Filters;
using qon.Functions.Propagators;
using qon.Machines;
using qon.QSL;
using qon.Variables;
using System;
using System.Linq;

namespace qon.Functions.Constraints
{
    public class Constraint<TQ> : IPreparation<TQ> where TQ : notnull
    {
        private readonly QLink<TQ>[] _links;
        private readonly Propagator<TQ> _propagator;

        public Constraint(QLink<TQ>[] links, Propagator<TQ> propagator)
        {
            _links = links;
            _propagator = propagator;
        }

        public Result Execute(Field<TQ> _)
        {
            return _propagator.ApplyTo(_links.Select(l => l.Object));
        }
    }
}
