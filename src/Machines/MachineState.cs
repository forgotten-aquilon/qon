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
        public void SetField(QObject<TQ>[] field)
        {
            Field.Update(field);
        }

        public int AddToField(QObject<TQ> @object)
        {
            return Field.Add(@object);
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder("{ ");

            var fieldRepresentation = Field.Select(v =>
                v.State != ValueState.Uncertain ? $"{v.Name}:[{v.Value}]" : $"{v.Name}:[{DomainLayer<TQ>.On(v).DescribeDomain()}]");

            result.AppendJoin(" ", fieldRepresentation);
            result.Append("}");

            return result.ToString();
        }
    }
}
