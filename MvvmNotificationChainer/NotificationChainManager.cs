using System;
using System.Collections.Generic;
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
    public class NotificationChainManager : INotificationChainManager
    {
        public Object ObservedObject { get; private set; }

        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Map of dependent property name to notification chain manager
        /// </summary>
        private Dictionary<String, INotificationChainManager> myDeepChainManagers = new Dictionary<String, INotificationChainManager> ();

        /// <summary>
        /// Map of dependent property name to function to get that property value
        /// </summary>
        private Dictionary<String, Func<Object, Object>> myDeepChainGetters = new Dictionary<String, Func<Object, Object>> ();

        /// <summary>
        /// Map of dependent property name to notification chain
        /// </summary>
        private Dictionary<String, NotificationChain> myChains = new Dictionary<String, NotificationChain> ();

        private List<NotificationChainCallback> myDefaultCallbacks = new List<NotificationChainCallback> ();

        // ReSharper disable once InconsistentNaming
        private readonly Object lock_Publish = new Object ();

        private PropertyChangedEventHandler myPropertyChangedEventHandler;

        private Action<PropertyChangedEventHandler> myRemovePropertyChangedEventHandler;

        public NotificationChainManager ()
        {
            myPropertyChangedEventHandler = Publish;
        }

        public NotificationChainManager (INotifyPropertyChanged notifyingObject) : this ()
        {
            notifyingObject.ThrowIfNull ("INotifyPropertyChanged");

            Observe (notifyingObject);
        }

        public NotificationChainManager (Object notifyingObject,
                                         Action<PropertyChangedEventHandler> addEventAction,
                                         Action<PropertyChangedEventHandler> removeEventAction) : this ()
        {
            Observe (notifyingObject, addEventAction, removeEventAction);
        }

        public virtual void Dispose ()
        {
            if (IsDisposed) return;

            ObservedObject = null;

            myRemovePropertyChangedEventHandler (myPropertyChangedEventHandler);
            myRemovePropertyChangedEventHandler = null;
            myPropertyChangedEventHandler = null;

            foreach (var ncm in myDeepChainManagers.Values)
                ncm.Dispose ();
            myDeepChainManagers.Clear ();
            myDeepChainManagers = null;

            myDeepChainGetters.Clear ();
            myDeepChainGetters = null;

            lock (myChains)
            {
                foreach (var chain in myChains.Values)
                    chain.Dispose ();
                myChains.Clear ();
                myChains = null;
            }

            myDefaultCallbacks.Clear ();
            myDefaultCallbacks = null;

            IsDisposed = true;
        }

        public void AddDefaultCall (Action onNotifyingPropertyChanged)
        {
            onNotifyingPropertyChanged.ThrowIfNull ("onNotifyingPropertyChanged");

            if (IsDisposed) return;

            AddDefaultCall ((sender, notifyingProperty, dependentProperty) => onNotifyingPropertyChanged ());
        }

        public void AddDefaultCall (NotificationChainCallback onNotifyingPropertyChanged)
        {
            onNotifyingPropertyChanged.ThrowIfNull ("onNotifyingPropertyChanged");

            if (IsDisposed) return;

            myDefaultCallbacks.Add (onNotifyingPropertyChanged);
        }

        public NotificationChain CreateOrGet<T1> (Expression<Func<T1>> propGetter)
        {
            propGetter.ThrowIfNull ("propGetter");

            if (IsDisposed) return null;

            // ReSharper disable once ExplicitCallerInfoArgument
            return CreateOrGet (propGetter.GetPropertyName ());
        }

        public NotificationChain CreateOrGet ([CallerMemberName] String dependentPropertyName = null)
        {
            dependentPropertyName.ThrowIfNull ("dependentPropertyName");

            if (IsDisposed) return null;

            NotificationChain chain;
            if (!myChains.TryGetValue (dependentPropertyName, out chain))
            {
                chain = myChains[dependentPropertyName] = new NotificationChain (this, dependentPropertyName);
                foreach (var callback in myDefaultCallbacks)
                    chain.AndCall (callback);
            }

            return chain;
        }

        // TODO consolidate CreateOrGetDeepManager methods? Only diff is propGetter

        public INotificationChainManager CreateOrGetManager<T1> (Expression<Func<T1>> propGetter)
            where T1 : class
        {
            var propName = propGetter.GetPropertyName ();

            INotificationChainManager mgr;
            if (!myDeepChainManagers.TryGetValue (propName, out mgr))
            {
                myDeepChainManagers[propName] = mgr = new NotificationChainManager ();
                myDeepChainGetters[propName] = _ => propGetter.Compile ().Invoke ();
            }

            return mgr;
        }

        public INotificationChainManager CreateOrGetManager<T0, T1> (Expression<Func<T0, T1>> propGetter)
            where T0 : INotifyPropertyChanged
            where T1 : class
        {
            var propName = propGetter.GetPropertyName ();

            INotificationChainManager mgr;
            if (!myDeepChainManagers.TryGetValue (propName, out mgr))
            {
                mgr = myDeepChainManagers[propName] = new NotificationChainManager ();
                myDeepChainGetters[propName] = parent => propGetter.Compile ().Invoke ((T0) parent);
            }

            return mgr;
        }

        public NotificationChain Get ([CallerMemberName] String dependentPropertyName = null)
        {
            dependentPropertyName.ThrowIfNull ("dependentPropertyName");

            if (IsDisposed) return null;

            NotificationChain chain;
            return myChains.TryGetValue (dependentPropertyName, out chain) ? chain : null;
        }

        public void Clear ([CallerMemberName] String dependentPropertyName = null)
        {
            dependentPropertyName.ThrowIfNull ("dependentPropertyName");

            if (IsDisposed) return;

            // ReSharper disable once ExplicitCallerInfoArgument
            var chain = Get (dependentPropertyName);
            if (chain == null) return;
            chain.Dispose ();
            myChains.Remove (dependentPropertyName);
        }

        public void Observe (INotifyPropertyChanged notifyingObject)
        {
            notifyingObject.ThrowIfNull ("notifyingObject");

            if (IsDisposed) return;

            Observe (notifyingObject, h => notifyingObject.PropertyChanged += h, h => notifyingObject.PropertyChanged -= h);
        }

        public void Observe (Object notifyingObject,
                             Action<PropertyChangedEventHandler> addEventAction,
                             Action<PropertyChangedEventHandler> removeEventAction)
        {
            notifyingObject.ThrowIfNull ("notifyingObject");
            addEventAction.ThrowIfNull ("addEventAction");
            removeEventAction.ThrowIfNull ("removeEventAction");

            if (IsDisposed) return;

            if (ReferenceEquals (ObservedObject, notifyingObject)) return;

            if (ObservedObject != null)
                throw new InvalidOperationException ("Can't observe a different object without calling StopObserving() first");

            ObservedObject = notifyingObject;

            NotificationChainPropertyAttribute.CallProperties (ObservedObject);

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

        public void Publish (Object sender, PropertyChangedEventArgs args)
        {
            sender.ThrowIfNull ("sender");

            if (IsDisposed) return;

            lock (lock_Publish)
            {
                foreach (var chain in myChains.Values)
                    chain.Publish (sender, args);

                INotificationChainManager manager;
                if (myDeepChainManagers.TryGetValue (args.PropertyName, out manager))
                {
                    var currentPropertyValue = (INotifyPropertyChanged) myDeepChainGetters[args.PropertyName] (sender);
                    if (currentPropertyValue != null)
                    {
                        if (ReferenceEquals (currentPropertyValue, manager.ObservedObject))
                            return; // no change

                        manager.StopObserving ();
                        manager.Observe (currentPropertyValue);
                    }
                    else
                    {
                        if (manager.ObservedObject == null)
                            return; // no change

                        manager.StopObserving ();
                    }
                }
            }
        }
    }
}