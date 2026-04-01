using qon.Exceptions;
using qon.Helpers;
using qon.Layers.StateLayers;
using qon.Layers.VariableLayers;
using qon.Machines;
using qon.Variables;
using qon.Variables.Domains;
using System;
using System.Collections.Generic;
using System.Text;

namespace qon.QSL
{
    public static partial class QMachineExtensions
    {
        public static QMachine<TQ> GenerateField<TQ>(this QMachine<TQ> machine, IDomain<TQ> domain, int count) where TQ : notnull
        {
            var field = new List<QObject<TQ>>();
            for (int i = 0; i < count; i++)
            {
                var variable = QObject<TQ>.Empty();
                DomainLayer<TQ>.GetOrCreate(variable).AssignDomain(domain);
                //field.Add(variable);

                machine.AddToField(variable);
            }
            //machine.InitializeField(field);

            return machine;
        }

        public static QMachine<TQ> GenerateField<TQ>(this QMachine<TQ> machine, IDomain<TQ> domain, IEnumerable<string> names) where TQ : notnull
        {
            var field = new List<QObject<TQ>>();
            foreach (var name in names)
            {
                var variable = QObject<TQ>.Empty(name);
                DomainLayer<TQ>.GetOrCreate(variable).AssignDomain(domain);
                //field.Add(variable);
                machine.AddToField(variable);
            }
            //machine.InitializeField(field);

            return machine;
        }
    }
}
