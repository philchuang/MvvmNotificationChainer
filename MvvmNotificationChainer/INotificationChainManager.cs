using System;
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
        Object ObservedObject { get; }

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
        /// Creates a new NotificationChain for the calling property, or returns an existing instance
        /// </summary>
        /// <param name="propGetter">Lambda expression for the property that depends on other properties</param>
        /// <returns></returns>
        NotificationChain CreateOrGet<T1> (Expression<Func<T1>> propGetter);

        /// <summary>
        /// Creates a new NotificationChain for the calling property, or returns an existing instance
        /// </summary>
        /// <param name="dependentPropertyName">Name of the property that depends on other properties</param>
        /// <returns></returns>
        NotificationChain CreateOrGet ([CallerMemberName] String dependentPropertyName = null);

        /// <summary>
        /// Creates a new NotificationChainManager for the calling property, or returns an existing instance
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="propGetter"></param>
        /// <returns></returns>
        INotificationChainManager<T1> CreateOrGetManager<T1> (Expression<Func<T1>> propGetter)
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
        /// Begins observing the given notifying object. If currently observing an object, must call StopObserving first.
        /// </summary>
        /// <param name="notifyingObject"></param>
        void Observe (INotifyPropertyChanged notifyingObject);

        /// <summary>
        /// Begins observing the given notifying object. If currently observing an object, must call StopObserving first.
        /// </summary>
        /// <param name="notifyingObject"></param>
        /// <param name="addEventAction"></param>
        /// <param name="removeEventAction"></param>
        void Observe (Object notifyingObject,
                      Action<PropertyChangedEventHandler> addEventAction,
                      Action<PropertyChangedEventHandler> removeEventAction);

        /// <summary>
        /// Stop observing the current notifying object.
        /// </summary>
        void StopObserving ();

        /// <summary>
        /// Pushes PropertyChangedEventArgs input for processing. Normally called by the PropertyChanged event of the current observed object.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns>whether or not the callbacks were triggered</returns>
        void Publish (Object sender, PropertyChangedEventArgs args);
    }

    /// <summary>
    /// Manages multiple NotificationChains for a single notifying parent object.
    /// Prevents duplication of NotificationChains by dependent property name.
    /// When disposing, calls Dispose on all NotificationChains.
    /// </summary>
    public interface INotificationChainManager<T> : INotificationChainManager
    {
        T Observed { get; }

        /// <summary>
        /// Begins observing the given notifying object. If currently observing an object, must call StopObserving first.
        /// </summary>
        /// <param name="notifyingObject"></param>
        void Observe<T0> (T0 notifyingObject)
            where T0 : T, INotifyPropertyChanged;

        /// <summary>
        /// Begins observing the given notifying object. If currently observing an object, must call StopObserving first.
        /// </summary>
        /// <param name="notifyingObject"></param>
        /// <param name="addEventAction"></param>
        /// <param name="removeEventAction"></param>
        void Observe (T notifyingObject,
                      Action<PropertyChangedEventHandler> addEventAction,
                      Action<PropertyChangedEventHandler> removeEventAction);

        /// <summary>
        /// Creates a new NotificationChainManager for the calling property, or returns an existing instance
        /// </summary>
        /// <typeparam name="T0"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="propGetter"></param>
        /// <returns></returns>
        INotificationChainManager<T1> CreateOrGetManager<T0, T1> (Expression<Func<T0, T1>> propGetter)
            where T0 : T
            where T1 : class;

        /// <summary>
        /// Pushes PropertyChangedEventArgs input for processing. Normally called by the PropertyChanged event of the current observed object.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns>whether or not the callbacks were triggered</returns>
        void Publish (T sender, PropertyChangedEventArgs args);
    }
}