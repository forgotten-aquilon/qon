using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using qon.Exceptions;
using qon.Layers;
using qon.Layers.VariableLayers;
using qon.Variables;

namespace qon.Machines
{
    /// <summary>
    /// "Hack" for searching for variables in the field by properties. Working without collection-expressions is not possible in Unity for now.
    /// I don't like recursive allocation, but by design solution machine is not supposed to run too often.
    /// FUTURE: Remove this as soon Unity supports recent .NET versions
    /// Or maybe keep, because public API looks much better this way.
    /// </summary>
    /// <typeparam name="TQ"></typeparam>
    public class SearchResult<TQ> where TQ : notnull
    {
        public IEnumerable<QVariable<TQ>> Result { get; set; }

        public SearchResult(IEnumerable<QVariable<TQ>> field)
        {
            Result = field;
        }

        public SearchResult<TQ> this[string name, object value]
        {
            get
            {
                ExceptionHelper.ThrowIfArgumentIsNull(name, nameof(name));
                return AsQuery().WithProperty(name, value).ToSearchResult();
            }
        }

        public SearchResult<TQ> this[Func<QVariable<TQ>, bool> predicate]
        {
            get
            {
                ExceptionHelper.ThrowIfArgumentIsNull(predicate, nameof(predicate));
                return AsQuery().Where(predicate).ToSearchResult();
            }
        }

        public static SearchResult<TQ> Search(IEnumerable<QVariable<TQ>> variables, string name, object value)
        {
            var result = variables.Where(x => object.Equals(x.GetNullOrValueProperty(name), value));
            return new SearchResult<TQ>(result);
        }

        public MachineStateQuery<TQ> AsQuery()
        {
            return new MachineStateQuery<TQ>(Result);
        }

        public static SearchResult<TQ> Search(IEnumerable<QVariable<TQ>> variables, Func<QVariable<TQ>, bool> predicate)
        {
            var result = variables.Where(predicate);

            return new SearchResult<TQ>(result);
        }
    }

    /// <summary>
    /// Wrapper class for working with current <see cref="Field{TQ}"/>
    /// </summary>
    /// <typeparam name="TQ"></typeparam>
    public class MachineState<TQ> : ILayerHolder<TQ, MachineState<TQ>> where TQ : notnull
    {
        /// <summary>
        /// Instance of the current Machine
        /// </summary>
        public QMachine<TQ> Machine { get; protected set; }

        /// <summary>
        /// Layers' manager, attached to <see cref="MachineState{TQ}"/>
        /// </summary>
        public LayersManager<TQ, MachineState<TQ>> LayerManager { get; set; }

        /// <summary>
        /// Current field of variables
        /// </summary>
        public Field<TQ> Field { get; protected set; }

        public MachineState(QMachine<TQ> machine)
        {
            Machine = machine;
            LayerManager = new LayersManager<TQ, MachineState<TQ>>(this);
            Field = new Field<TQ>(machine);
        }

        /// <summary>
        /// Updates variables of the current <see cref="Field"/>
        /// </summary>
        /// <param name="field"></param>
        public void SetField(QVariable<TQ>[] field)
        {
            Field.Update(field);
        }

        #region Queries

        public SearchResult<TQ> this[string name, object value]
        {
            get
            {
                ExceptionHelper.ThrowIfArgumentIsNull(name, nameof(name));
                return Query().WithProperty(name, value).ToSearchResult();
            }
        }

        public SearchResult<TQ> this[Func<QVariable<TQ>, bool> predicate]
        {
            get
            {
                ExceptionHelper.ThrowIfArgumentIsNull(predicate, nameof(predicate));
                return Query().Where(predicate).ToSearchResult();
            }
        }

        public MachineStateQuery<TQ> Query()
        {
            return new MachineStateQuery<TQ>(Field.Variables);
        }

        #endregion

        public override string ToString()
        {
            StringBuilder result = new StringBuilder("{ ");

            var fieldRepresentation = Field.Select(v =>
                v.State != ValueState.Uncertain ? $"{v.Name}:[{v.Value}]" : $"{v.Name}:[{DomainLayer<TQ>.With(v).DescribeDomain()}]");

            result.AppendJoin(" ", fieldRepresentation);
            result.Append("}");

            return result.ToString();
        }
    }
}
