using qon.Exceptions;
using qon.Layers.VariableLayers;
using qon.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Machines;

namespace qon.Layers
{
    /// <summary>
    /// Base class for layers
    /// </summary>
    public abstract class BaseLayer
    {
        /// <summary>
        /// Is used in sorting layers, while performing relevant operations
        /// </summary>
        public int PriorityIndex { get; set; } = 0;
    }

    /// <summary>
    /// Generic base class for layers, is used for Curiously Recurring Template Pattern implementation
    /// </summary>
    /// <typeparam name="TQ">Key generic parameter</typeparam>
    /// <typeparam name="TSelf">
    /// Generic parameter used for Curiously Recurring Template Pattern implementation
    /// </typeparam>
    /// <typeparam name="THolder">
    /// Type, which can hold this layer, e.g. <see cref="QObject{TQ}"/> or <see cref="MachineState{TQ}"/>
    /// </typeparam>
    public abstract class BaseLayer<TQ, TSelf, THolder> : BaseLayer, ILayer<TQ, THolder> where TQ : notnull
        where TSelf : BaseLayer<TQ, TSelf, THolder>, ILayer<TQ, THolder>, new()
        where THolder : class, ILayerHolder<TQ, THolder>
    {

        /// <summary>
        /// Nullable reference to <see cref="LayersManager{TQ,THolder}"/> . Allows late binding to actual instance of
        /// manager.
        /// </summary>
        protected LayersManager<TQ, THolder>? NullableManager { get; set; }


        /// <summary>
        /// Non-nullable reference to <see cref="LayersManager{TQ,THolder}"/>, which is checked in runtime. Allows late binding to actual
        /// instance of manager.
        /// </summary>
        public LayersManager<TQ, THolder> Manager => ExceptionHelper.ThrowIfFieldIsNull(NullableManager, nameof(NullableManager));

        /// <summary>
        /// Reference to an instance of type, which can hold this layer. 
        /// </summary>
        public THolder Holder => ExceptionHelper.ThrowIfFieldIsNull(Manager.Holder, nameof(Manager.Holder));

        /// <summary>
        /// Non-nullable reference to Solution Machine, which is checked in runtime. Allows late binding to actual
        /// instance of machine.
        /// </summary>
        public QMachine<TQ> Machine => ExceptionHelper.ThrowIfFieldIsNull(Holder.Machine, nameof(Holder.Machine));

        /// <summary>
        /// Returns existing instance of this layer. If Holder object doesn't have layer of this type, exception is
        /// thrown
        /// </summary>
        /// <param name="holder"></param>
        /// <returns></returns>
        /// <exception cref="InternalNullException"></exception>
        public static TSelf On(THolder holder)
        {
            holder.LayerManager.TryGetLayer<TSelf>(out var layer);

            ExceptionHelper.ThrowIfInternalValueIsNull(layer, nameof(layer));

            return layer;
        }

        /// <summary>
        /// Returns nullable reference of this layer from Holder object
        /// </summary>
        /// <param name="holder"></param>
        /// <returns></returns>
        public static TSelf? From(THolder holder)
        {
            holder.LayerManager.TryGetLayer<TSelf>(out TSelf? layer);
            return layer;
        }

        /// <summary>
        /// Creates and/or returns instance of this layer. 
        /// </summary>
        /// <param name="holder"></param>
        /// <returns></returns>
        public static TSelf GetOrCreate(THolder holder)
        {
            if (!holder.LayerManager.TryGetLayer<TSelf>(out var layer))
            {
                layer = new TSelf();
                holder.LayerManager.Add(layer);
                layer.NullableManager = holder.LayerManager;
            }

            return layer;
        }

        #region Implementation of ICopy<ILayer<T,THolder>>

        public abstract ILayer<TQ, THolder> Copy();

        #endregion

        #region Implementation of ILayer<T,THolder>

        /// <summary>
        /// Changes instance of <see cref="LayersManager{TQ,THolder}"/>.
        /// </summary>
        /// <param name="manager"></param>
        public void UpdateManager(LayersManager<TQ, THolder> manager)
        {
            NullableManager = manager;
        }

        #endregion

        public override int GetHashCode()
        {
            return (NullableManager is not null ? NullableManager.GetHashCode() : 0);
        }

        public virtual bool Equals(ILayer<TQ, THolder> other)
        {
            if (ReferenceEquals(this, other)) return true;

            if (this.GetType() != other.GetType()) return false;

            return true;
        }
    }
}
