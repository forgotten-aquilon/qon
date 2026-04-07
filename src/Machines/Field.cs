using qon.Helpers;
using qon.Layers.VariableLayers;
using qon.Variables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using qon.QSL;
using qon.Exceptions;

namespace qon.Machines
{
    /// <summary>
    /// Collection of <see cref="QObject{TQ}"/>
    /// </summary>
    /// <typeparam name="TQ"></typeparam>
    public class Field<TQ> : ICopy<Field<TQ>>, IEnumerable<QObject<TQ>> where TQ : notnull
    {
        /// <summary>
        /// All variables of the current Field
        /// </summary>
        public QObject<TQ>[] Variables { get; protected set; }

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
            Variables = Array.Empty<QObject<TQ>>();
        }

        /// <summary>
        /// Initialize new Field with provided Variables, binds it to Solution Machine
        /// </summary>
        /// <param name="machine"></param>
        /// <param name="variables"></param>
        public Field(QMachine<TQ> machine, QObject<TQ>[] variables)
        {
            Machine = machine;
            Variables = variables;
        }

        /// <summary>
        /// Overwrites variables of Field with new ones
        /// </summary>
        /// <param name="variables"></param>
        public void Update(QObject<TQ>[] variables)
        {
            Variables = variables;
        }

        public void Update(Field<TQ> anotherField)
        {
            Variables = anotherField.Variables;
        }

        public void UpdateWithValues(Optional<TQ>[] values)
        {
            ExceptionHelper.ThrowIfPredicateFalse(values.Length, length => length == Count, "Value count should match field size.");

            for (int i = 0; i < values.Length; i++)
            {
                Variables[i].Value = values[i];
            }
        }

        public int Add(QObject<TQ> @object)
        {
            var tempList = Variables.ToList();

            tempList.Add(@object);

            Variables = tempList.ToArray();

            return Variables.Length - 1;
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
            QObject<TQ>[] fieldCopy = new QObject<TQ>[Count];
            Array.Copy(Variables, fieldCopy, Count);
            Field<TQ> newField = new Field<TQ>(Machine, fieldCopy);

            return newField;
        }

        /// <summary>
        /// Return <see cref="QObject{TQ}"/> from the Field based on its numerical position in the array
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public QObject<TQ> this[int index]
        {
            get => Variables[index];
            set => Variables[index] = value;
        }

        /// <summary>
        /// Return <see cref="QObject{TQ}"/> from the Field based on its name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public QObject<TQ> this[string name]
        {
            get => Variables[Machine.NamedIndexer[name]];
            set => Variables[Machine.NamedIndexer[name]] = value;
        }

        /// <summary>
        /// Returns <see cref="QObject{TQ}"/> from the Field based on its <see cref="Guid"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public QObject<TQ> this[Guid id]
        {
            get => Variables[Machine.GuidIndexer[id]];
            set => Variables[Machine.GuidIndexer[id]] = value;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder("{ ");

            var fieldRepresentation = Variables.Select(v =>
                v.OnDomainLayer().State != ValueState.Uncertain ? $"{v.Value}" : $"{v.Name}:[{v.OnDomainLayer().DescribeDomain()}]");

            result.AppendJoin(" ", fieldRepresentation);
            result.Append("}");

            return result.ToString();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            if (obj is Field<TQ> other)
            {
                return this.SequenceEqual(other);
            }

            return base.Equals(obj);
        }

        public IEnumerator<QObject<TQ>> GetEnumerator()
        {
            return Variables.AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
