using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
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
        public IEnumerable<Object> ObservedObjects { get { return myObservedObjects.Keys.Select (o => o); } }

        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Map of an observed object to the delegate to remove the handler for it
        /// </summary>
        private Dictionary<Object, Action> myObservedObjects = new Dictionary<object, Action> ();

        /// <summary>
        /// Map of dependent property name to notification chain manager
        /// </summary>
        private Dictionary<String, INotificationChainManager> myDeepChainManagers = new Dictionary<String, INotificationChainManager> ();

        /// <summary>
        /// Map of dependent property name to function to get that property value
        /// </summary>
        private Dictionary<String, Func<Object, Object>> myDeepChainGetters = new Dictionary<String, Func<Object, Object>> ();

        /// <summary>
        /// Map of an observed object to a map of property name to property value
        /// </summary>
        private Dictionary<Object, Dictionary<String, INotifyPropertyChanged>> myDeepPreviousObservedValues = new Dictionary<object, Dictionary<string, INotifyPropertyChanged>> ();

        /// <summary>
        /// Map of dependent property name to notification chain
        /// </summary>
        private Dictionary<String, NotificationChain> myChains = new Dictionary<String, NotificationChain> ();

        private List<NotificationChainCallback> myDefaultCallbacks = new List<NotificationChainCallback> ();

        // ReSharper disable once InconsistentNaming
        private readonly Object lock_Publish = new Object ();

        private PropertyChangedEventHandler myPropertyChangedEventHandler;

        public NotificationChainManager ()
        {
            myPropertyChangedEventHandler = Publish;
        }

        public NotificationChainManager (INotifyPropertyChanged notifyingObject)
            : this ()
        {
            notifyingObject.ThrowIfNull ("INotifyPropertyChanged");

            ObserveINotifyPropertyChanged (notifyingObject);
        }

        public NotificationChainManager (
            Object notifyingObject,
            Action<PropertyChangedEventHandler> addEventAction,
            Action<PropertyChangedEventHandler> removeEventAction)
            : this ()
        {
            ObservePropertyChangedEventHandlers (notifyingObject, addEventAction, removeEventAction);
        }

        public virtual void Dispose ()
        {
            if (IsDisposed) return;

            foreach (var kvp in myObservedObjects)
                kvp.Value ();
            myObservedObjects.Clear ();
            myObservedObjects = null;

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

        public ICollectionNotificationChainManager CreateOrGetCollectionManager<T1> (Expression<Func<ObservableCollection<T1>>> collectionPropGetter)
            where T1 : class
        {
            var propName = collectionPropGetter.GetPropertyName ();

            INotificationChainManager mgr;
            if (!myDeepChainManagers.TryGetValue (propName, out mgr))
            {
                myDeepChainManagers[propName] = mgr = new CollectionNotificationChainManager ();
                myDeepChainGetters[propName] = parent => collectionPropGetter.Compile ().Invoke ();
            }

            return (ICollectionNotificationChainManager) mgr;
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

        public virtual void Observe (Object notifyingObject)
        {
            notifyingObject.ThrowIfNull ("notifyingObject");

            if (IsDisposed) return;

            if (notifyingObject is INotifyPropertyChanged)
            {
                Observe ((INotifyPropertyChanged) notifyingObject);
                return;
            }

            throw new InvalidOperationException ("Unable to observe an object of type \"{0}\"".FormatWith (notifyingObject.GetType ().FullName));
        }

        public virtual void Observe (INotifyPropertyChanged notifyingObject)
        {
            ObserveINotifyPropertyChanged (notifyingObject);
        }

        private void ObserveINotifyPropertyChanged (INotifyPropertyChanged notifyingObject)
        {
            notifyingObject.ThrowIfNull ("notifyingObject");

            if (IsDisposed) return;

            Observe (notifyingObject, h => notifyingObject.PropertyChanged += h, h => notifyingObject.PropertyChanged -= h);
        }

        public virtual void Observe (
            Object notifyingObject,
            Action<PropertyChangedEventHandler> addEventAction,
            Action<PropertyChangedEventHandler> removeEventAction)
        {
            ObservePropertyChangedEventHandlers (notifyingObject, addEventAction, removeEventAction);
        }

        private void ObservePropertyChangedEventHandlers (
            Object notifyingObject,
            Action<PropertyChangedEventHandler> addEventAction,
            Action<PropertyChangedEventHandler> removeEventAction)
        {
            notifyingObject.ThrowIfNull ("notifyingObject");
            addEventAction.ThrowIfNull ("addEventAction");
            removeEventAction.ThrowIfNull ("removeEventAction");

            if (IsDisposed) return;

            if (myObservedObjects.ContainsKey (notifyingObject)) return;

            myObservedObjects[notifyingObject] = () => removeEventAction (myPropertyChangedEventHandler);

            NotificationChainPropertyAttribute.CallProperties (notifyingObject);

            addEventAction (myPropertyChangedEventHandler);
        }

        public virtual void StopObserving (Object notifyingObject)
        {
            notifyingObject.ThrowIfNull ("notifyingObject");

            if (IsDisposed) return;

            Action removeHandler;
            if (!myObservedObjects.TryGetValue (notifyingObject, out removeHandler))
                return;

            lock (this)
            {
                removeHandler ();
                myObservedObjects.Remove (notifyingObject);
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
                if (!myDeepChainManagers.TryGetValue (args.PropertyName, out manager)) return;

                INotifyPropertyChanged previousPropertyValue = null;
                if (myDeepPreviousObservedValues.ContainsKey (sender))
                    myDeepPreviousObservedValues[sender].TryGetValue (args.PropertyName, out previousPropertyValue);
                var currentPropertyValue = (INotifyPropertyChanged) myDeepChainGetters[args.PropertyName] (sender);

                if (ReferenceEquals (previousPropertyValue, currentPropertyValue)) return;

                if (previousPropertyValue != null)
                {
                    if (manager is ICollectionNotificationChainManager)
                        ((ICollectionNotificationChainManager) manager).StopObservingCollection ((INotifyCollectionChanged) previousPropertyValue);
                    else
                        manager.StopObserving (previousPropertyValue);
                }

                if (currentPropertyValue != null)
                {
                    if (manager is ICollectionNotificationChainManager)
                        ((ICollectionNotificationChainManager) manager).ObserveCollection ((INotifyCollectionChanged) currentPropertyValue);
                    else
                        manager.Observe (currentPropertyValue);
                }
            }
        }
    }
}