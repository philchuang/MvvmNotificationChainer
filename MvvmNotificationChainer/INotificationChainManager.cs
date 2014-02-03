using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Com.PhilChuang.Utils.MvvmNotificationChainer
{
    /// <summary>
    /// Manages multiple NotificationChains for a single notifying parent object.
    /// Prevents duplication of NotificationChains by dependent property name.
    /// When disposing, calls Dispose on all NotificationChains.
    /// </summary>
    public interface INotificationChainManager : IDisposable
    {
        IEnumerable<Object> ObservedObjects { get; }

        bool IsDisposed { get; }

        /// <summary>
        /// Specifies a default action to add to a new NotificationChain.
        /// </summary>
        /// <param name="onNotifyingPropertyChanged"></param>
        /// <returns></returns>
        void AddDefaultCall (Action onNotifyingPropertyChanged);

        /// <summary>
        /// Specifies an action to invoke when a notifying property is changed. Multiple actions can be invoked.
        /// </summary>
        /// <param name="onNotifyingPropertyChanged"></param>
        /// <returns></returns>
        void AddDefaultCall (NotificationChainCallback onNotifyingPropertyChanged);

        /// <summary>
        /// Creates a new NotificationChain for the given property, or returns an existing instance
        /// </summary>
        /// <param name="propGetter">Lambda expression for the property that depends on other properties</param>
        /// <returns></returns>
        NotificationChain CreateOrGet<T1> (Expression<Func<T1>> propGetter);

        /// <summary>
        /// Creates a new NotificationChain for the given property, or returns an existing instance
        /// </summary>
        /// <param name="dependentPropertyName">Name of the property that depends on other properties</param>
        /// <returns></returns>
        NotificationChain CreateOrGet ([CallerMemberName] String dependentPropertyName = null);

        /// <summary>
        /// Creates a new NotificationChainManager for the given property, or returns an existing instance
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="propGetter"></param>
        /// <returns></returns>
        INotificationChainManager CreateOrGetManager<T1> (Expression<Func<T1>> propGetter)
            where T1 : class;

        /// <summary>
        /// Creates a new NotificationChainManager for the given property, or returns an existing instance
        /// </summary>
        /// <typeparam name="T0"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="propGetter"></param>
        /// <returns></returns>
        INotificationChainManager CreateOrGetManager<T0, T1> (Expression<Func<T0, T1>> propGetter)
            where T0 : INotifyPropertyChanged
            where T1 : class;

        /// <summary>
        /// Creates a new NotificationChainManager for the given collection property, or returns an existing instance
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="collectionPropGetter"></param>
        /// <returns></returns>
        ICollectionNotificationChainManager CreateOrGetCollectionManager<T1> (Expression<Func<ObservableCollection<T1>>> collectionPropGetter)
            where T1 : class;

        /// <summary>
        /// Creates a new NotificationChain for the calling property
        /// </summary>
        /// <param name="dependentPropertyName">Name of the property that depends on other properties</param>
        /// <returns></returns>
        NotificationChain Get ([CallerMemberName] String dependentPropertyName = null);

        /// <summary>
        /// Clears a NotificationChain for the calling property
        /// </summary>
        /// <param name="dependentPropertyName">Name of the property that depends on other properties</param>
        /// <returns></returns>
        void Clear ([CallerMemberName] String dependentPropertyName = null);

        /// <summary>
        /// Attempt to determine how to observe the given notifying object, then begin observing it.
        /// </summary>
        /// <param name="notifyingObject"></param>
        void Observe (Object notifyingObject);

        /// <summary>
        /// Begins observing the given notifying object.
        /// </summary>
        /// <param name="notifyingObject"></param>
        void Observe (INotifyPropertyChanged notifyingObject);

        /// <summary>
        /// Begins observing the given notifying object.
        /// </summary>
        /// <param name="notifyingObject"></param>
        /// <param name="addEventAction"></param>
        /// <param name="removeEventAction"></param>
        void Observe (Object notifyingObject,
                      Action<PropertyChangedEventHandler> addEventAction,
                      Action<PropertyChangedEventHandler> removeEventAction);

        /// <summary>
        /// Stop observing the given notifying object.
        /// </summary>
        /// <param name="notifyingObject"></param>
        void StopObserving (Object notifyingObject);

        /// <summary>
        /// Pushes PropertyChangedEventArgs input for processing. Normally called by the PropertyChanged event of the current observed object.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns>whether or not the callbacks were triggered</returns>
        void Publish (Object sender, PropertyChangedEventArgs args);
    }
}