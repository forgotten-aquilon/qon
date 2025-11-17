using qon.Helpers;
using qon.Layers.VariableLayers;
using qon.Machines;
using qon.Variables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qon
{
    public class Field<TQ> : ICopy<Field<TQ>>, IEnumerable<QVariable<TQ>> where TQ : notnull
    {
        public QVariable<TQ>[] Variables { get; protected set; }
        public readonly QMachine<TQ> Machine;    
        public int Count => Variables.Length;

        public Field(QMachine<TQ> machine)
        {
            Machine = machine;
            Variables = Array.Empty<QVariable<TQ>>();
        }

        public Field(QMachine<TQ> machine, QVariable<TQ>[] variables)
        {
            Machine = machine;
            Variables = variables;
        }

        public void Update(QVariable<TQ>[] variables)
        {
            Variables = variables;
        }

        public void Update(Field<TQ> anotherField)
        {
            Variables = anotherField.Variables;
        }

        public Field<TQ> Copy()
        {
            return new Field<TQ>(Machine, Variables.Select(x => x.Copy()).ToArray());
        }

        public QVariable<TQ> this[int index]
        {
            get => Variables[index];
            set => Variables[index] = value;
        }

        public QVariable<TQ> this[string name]
        {
            get => Variables[Machine.NamedIndexer[name]];
            set => Variables[Machine.NamedIndexer[name]] = value;
        }

        public QVariable<TQ> this[Guid id]
        {
            get => Variables[Machine.GuidIndexer[id]];
            set => Variables[Machine.GuidIndexer[id]] = value;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder("{ ");

            var fieldRepresentation = Variables.Select(v =>
                v.State != ValueState.Uncertain ? $"{v.Value}" : $"{v.Name}:[{DomainLayer<TQ>.With(v).DescribeDomain()}]");

            result.AppendJoin(" ", fieldRepresentation);
            result.Append("}");

            return result.ToString();
        }

        public IEnumerator<QVariable<TQ>> GetEnumerator()
        {
            return Variables.AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
