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
    public class Field<T> : ICopy<Field<T>>, IEnumerable<QVariable<T>>
    {
        public QVariable<T>[] Variables { get; protected set; }
        public readonly QMachine<T> Machine;    
        public int Count => Variables.Length;

        public Field(QMachine<T> machine)
        {
            Machine = machine;
            Variables = Array.Empty<QVariable<T>>();
        }

        public Field(QMachine<T> machine, QVariable<T>[] variables)
        {
            Machine = machine;
            Variables = variables;
        }

        public void Update(QVariable<T>[] variables)
        {
            Variables = variables;
        }

        public void Update(Field<T> anotherField)
        {
            Variables = anotherField.Variables;
        }

        public Field<T> Copy()
        {
            return new Field<T>(Machine, Variables.Select(x => x.Copy()).ToArray());
        }

        public QVariable<T> this[int index]
        {
            get => Variables[index];
            set => Variables[index] = value;
        }

        public QVariable<T> this[string name]
        {
            get => Variables[Machine.NamedIndexer[name]];
            set => Variables[Machine.NamedIndexer[name]] = value;
        }

        public QVariable<T> this[Guid id]
        {
            get => Variables[Machine.GuidIndexer[id]];
            set => Variables[Machine.GuidIndexer[id]] = value;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder("{ ");

            var fieldRepresentation = Variables.Select(v =>
                v.State != ValueState.Uncertain ? $"{v.Value}" : $"{v.Name}:[{DomainLayer<T>.With(v).DescribeDomain()}]");

            result.AppendJoin(" ", fieldRepresentation);
            result.Append("}");

            return result.ToString();
        }

        public IEnumerator<QVariable<T>> GetEnumerator()
        {
            return Variables.AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
