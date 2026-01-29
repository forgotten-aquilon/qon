using qon.Helpers;
using qon.Layers.VariableLayers;
using qon.Variables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace qon.Machines
{
    /// <summary>
    /// Collection of <see cref="QVariable{TQ}"/>
    /// </summary>
    /// <typeparam name="TQ"></typeparam>
    public class Field<TQ> : ICopy<Field<TQ>>, IEnumerable<QVariable<TQ>> where TQ : notnull
    {
        /// <summary>
        /// All variables of the current Field
        /// </summary>
        public QVariable<TQ>[] Variables { get; protected set; }

        /// <summary>
        /// Instance of the current Machine
        /// </summary>
        public QMachine<TQ> Machine { get; private set; }    

        /// <summary>
        /// Amount of <see cref="Variables"/>
        /// </summary>
        public int Count => Variables.Length;

        /// <summary>
        /// Creates new Field and binds it to Solution Machine
        /// </summary>
        /// <param name="machine"></param>
        public Field(QMachine<TQ> machine)
        {
            Machine = machine;
            Variables = Array.Empty<QVariable<TQ>>();
        }

        /// <summary>
        /// Initialize new Field with provided Variables, binds it to Solution Machine 
        /// </summary>
        /// <param name="machine"></param>
        /// <param name="variables"></param>
        public Field(QMachine<TQ> machine, QVariable<TQ>[] variables)
        {
            Machine = machine;
            Variables = variables;
        }

        /// <summary>
        /// Overwrites variables of Field with new ones
        /// </summary>
        /// <param name="variables"></param>
        public void Update(QVariable<TQ>[] variables)
        {
            Variables = variables;
        }

        public void Update(Field<TQ> anotherField)
        {
            Variables = anotherField.Variables;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Field<TQ> Copy()
        {
            return new Field<TQ>(Machine, Variables.Select(x => x.Copy()).ToArray());
        }

        /// <summary>
        /// Creates new instance of Field, which contains references to existing variables of the current Field
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Field<TQ> ShallowCopy()
        {
            QVariable<TQ>[] fieldCopy = new QVariable<TQ>[Count];
            Array.Copy(Variables, fieldCopy, Count);
            Field<TQ> newField = new Field<TQ>(Machine, fieldCopy);

            return newField;
        }

        /// <summary>
        /// Return <see cref="QVariable{TQ}"/> from the Field based on its numerical position in the array
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public QVariable<TQ> this[int index]
        {
            get => Variables[index];
            set => Variables[index] = value;
        }

        /// <summary>
        /// Return <see cref="QVariable{TQ}"/> from the Field based on its name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public QVariable<TQ> this[string name]
        {
            get => Variables[Machine.NamedIndexer[name]];
            set => Variables[Machine.NamedIndexer[name]] = value;
        }

        /// <summary>
        /// Returns <see cref="QVariable{TQ}"/> from the Field based on its <see cref="Guid"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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
