using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;

namespace Com.PhilChuang.Utils.MvvmNotificationChainer
{
    /// <summary>
    /// Manages multiple NotificationChains for a single notifying parent object.
    /// Prevents duplication of NotificationChains by dependent property name.
    /// When disposing, calls Dispose on all NotificationChains.
    /// </summary>
    public class NotificationChainManager : IDisposable
    {
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Map of dependent property name to notification chain
        /// </summary>
        private Dictionary<String, NotificationChain> myChains = new Dictionary<string, NotificationChain> ();

        private List<NotificationChainCallback> myDefaultCallbacks = new List<NotificationChainCallback> ();

        private PropertyChangedEventHandler myPropertyChangedEventHandler;

        public Object ObservedObject
        { get; private set; }

        private Action<PropertyChangedEventHandler> myRemovePropertyChangedEventHandler;

        public NotificationChainManager ()
        {
            myPropertyChangedEventHandler = OnNotifyingObjectPropertyChanged;
        }

        public NotificationChainManager (INotifyPropertyChanged notifyingObject) : this ()
        {
            Observe (notifyingObject);
        }

        public NotificationChainManager (Object notifyingObject,
                                         Action<PropertyChangedEventHandler> addEventAction,
                                         Action<PropertyChangedEventHandler> removeEventAction) : this ()
        {
            Observe (notifyingObject, addEventAction, removeEventAction);
        }

        public void Observe (INotifyPropertyChanged notifyingObject)
        {
            notifyingObject.ThrowIfNull("notifyingObject");

            if (IsDisposed) return;

            Observe (notifyingObject, h => notifyingObject.PropertyChanged += h, h => notifyingObject.PropertyChanged -= h);
        }

        public void Observe (Object notifyingObject,
                             Action<PropertyChangedEventHandler> addEventAction,
                             Action<PropertyChangedEventHandler> removeEventAction)
        {
            notifyingObject.ThrowIfNull ("notifyingObject");

            if (IsDisposed) return;

            if (ReferenceEquals (ObservedObject, notifyingObject)) return;

            if (ObservedObject != null)
                throw new InvalidOperationException ("Can't observe a different object without calling StopObserving() first");

            addEventAction (myPropertyChangedEventHandler);
            myRemovePropertyChangedEventHandler = removeEventAction;
        }

        public void StopObserving ()
        {
            if (IsDisposed) return;

            if (ObservedObject == null) return;

            lock (this)
            {
                myRemovePropertyChangedEventHandler (myPropertyChangedEventHandler);
                myRemovePropertyChangedEventHandler = null;
                ObservedObject = null;
            }
        }

        public virtual void Dispose ()
        {
            if (IsDisposed) return;

            ObservedObject = null;

            myRemovePropertyChangedEventHandler (myPropertyChangedEventHandler);
            myRemovePropertyChangedEventHandler = null;
            myPropertyChangedEventHandler = null;

            lock (myChains)
            {
                foreach (var chain in myChains.Values)
                    chain.Dispose ();
                myChains.Clear ();
                myChains = null;
            }

            myDefaultCallbacks.Clear();
            myDefaultCallbacks = null;
            
            IsDisposed = true;
        }

        private void OnNotifyingObjectPropertyChanged (Object sender, PropertyChangedEventArgs args)
        {
            if (IsDisposed) return;

            lock (myChains)
            {
                foreach (var chain in myChains.Values)
                    chain.Publish (sender, args);
            }
        }

        /// <summary>
        /// Specifies a default action to add to a new NotificationChain.
        /// </summary>
        /// <param name="onNotifyingPropertyChanged"></param>
        /// <returns></returns>
        public void AddDefaultCall (Action onNotifyingPropertyChanged)
        {
            onNotifyingPropertyChanged.ThrowIfNull ("onNotifyingPropertyChanged");

            if (IsDisposed) return;

            AddDefaultCall ((sender, notifyingProperty, dependentProperty) => onNotifyingPropertyChanged ());
        }

        /// <summary>
        /// Specifies an action to invoke when a notifying property is changed. Multiple actions can be invoked.
        /// </summary>
        /// <param name="onNotifyingPropertyChanged"></param>
        /// <returns></returns>
        public void AddDefaultCall (NotificationChainCallback onNotifyingPropertyChanged)
        {
            onNotifyingPropertyChanged.ThrowIfNull ("onNotifyingPropertyChanged");

            if (IsDisposed) return;

            myDefaultCallbacks.Add (onNotifyingPropertyChanged);
        }

        /// <summary>
        /// Creates a new NotificationChain for the calling property, or returns an existing instance
        /// </summary>
        /// <param name="propGetter">Lambda expression for the property that depends on other properties</param>
        /// <returns></returns>
        public NotificationChain CreateOrGet<T> (Expression<Func<T>> propGetter)
        {
            propGetter.ThrowIfNull ("propGetter");

            if (IsDisposed) return null;

            return CreateOrGet (propGetter.GetPropertyName ());
        }

        /// <summary>
        /// Creates a new NotificationChain for the calling property, or returns an existing instance
        /// </summary>
        /// <param name="dependentPropertyName">Name of the property that depends on other properties</param>
        /// <returns></returns>
        public NotificationChain CreateOrGet ([CallerMemberName] string dependentPropertyName = null)
        {
            dependentPropertyName.ThrowIfNull ("dependentPropertyName");

            if (IsDisposed) return null;

            NotificationChain chain;
            if (!myChains.TryGetValue (dependentPropertyName, out chain))
            {
                chain = myChains[dependentPropertyName] = new NotificationChain (dependentPropertyName);
                foreach (var callback in myDefaultCallbacks)
                    chain.AndCall (callback);
            }

            return chain;
        }

        /// <summary>
        /// Creates a new NotificationChain for the calling property
        /// </summary>
        /// <param name="dependentPropertyName">Name of the property that depends on other properties</param>
        /// <returns></returns>
        public NotificationChain Get ([CallerMemberName] string dependentPropertyName = null)
        {
            dependentPropertyName.ThrowIfNull ("dependentPropertyName");

            if (IsDisposed) return null;

            NotificationChain chain;
            return myChains.TryGetValue (dependentPropertyName, out chain) ? chain : null;
        }

        /// <summary>
        /// Clears a NotificationChain for the calling property
        /// </summary>
        /// <param name="dependentPropertyName">Name of the property that depends on other properties</param>
        /// <returns></returns>
        public void Clear ([CallerMemberName] string dependentPropertyName = null)
        {
            dependentPropertyName.ThrowIfNull ("dependentPropertyName");

            if (IsDisposed) return;

            var chain = Get(dependentPropertyName);
            if (chain == null) return;
            chain.Dispose ();
            myChains.Remove (dependentPropertyName);
        }
    }
}