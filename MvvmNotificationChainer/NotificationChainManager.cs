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
    public class NotificationChainManager
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
        private Dictionary<String, NotificationChainManager> myDeepChainManagers = new Dictionary<String, NotificationChainManager> ();

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

            Observe (notifyingObject);
        }

        public NotificationChainManager (
            Object notifyingObject,
            Action<PropertyChangedEventHandler> addEventAction,
            Action<PropertyChangedEventHandler> removeEventAction)
            : this ()
        {
            Observe (notifyingObject, addEventAction, removeEventAction);
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
        /// Creates a new NotificationChain for the given property, or returns an existing instance
        /// </summary>
        /// <param name="propGetter">Lambda expression for the property that depends on other properties</param>
        /// <returns></returns>
        public NotificationChain CreateOrGet<T1> (Expression<Func<T1>> propGetter)
        {
            propGetter.ThrowIfNull ("propGetter");

            if (IsDisposed) return null;

            // ReSharper disable once ExplicitCallerInfoArgument
            return CreateOrGet (propGetter.GetPropertyOrFieldName ());
        }

        /// <summary>
        /// Creates a new NotificationChain for the given property, or returns an existing instance
        /// </summary>
        /// <param name="dependentPropertyName">Name of the property that depends on other properties</param>
        /// <returns></returns>
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
        
        /// <summary>
        /// Creates a new NotificationChainManager for the given property, or returns an existing instance
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="propGetter"></param>
        /// <returns></returns>
        internal NotificationChainManager CreateOrGetManager<T1> (Expression<Func<T1>> propGetter)
            where T1 : class
        {
            var propName = propGetter.GetPropertyOrFieldName ();

            NotificationChainManager mgr;
            if (!myDeepChainManagers.TryGetValue (propName, out mgr))
            {
                myDeepChainManagers[propName] = mgr = new NotificationChainManager ();
                myDeepChainGetters[propName] = _ => propGetter.Compile ().Invoke ();
                var currentValue = (T1) myDeepChainGetters[propName] (null);
                if (currentValue != null)
                    mgr.Observe (currentValue);
            }

            return mgr;
        }

        /// <summary>
        /// Creates a new NotificationChainManager for the given property, or returns an existing instance
        /// </summary>
        /// <typeparam name="T0"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="propGetter"></param>
        /// <returns></returns>
        internal NotificationChainManager CreateOrGetManager<T0, T1> (Expression<Func<T0, T1>> propGetter)
            where T0 : INotifyPropertyChanged
            where T1 : class
        {
            var propName = propGetter.GetPropertyOrFieldName ();

            NotificationChainManager mgr;
            if (!myDeepChainManagers.TryGetValue (propName, out mgr))
            {
                mgr = myDeepChainManagers[propName] = new NotificationChainManager ();
                myDeepChainGetters[propName] = parent => propGetter.Compile ().Invoke ((T0) parent);
            }

            return mgr;
        }

        /// <summary>
        /// Creates a new NotificationChainManager for the given collection property, or returns an existing instance
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="collectionPropGetter"></param>
        /// <returns></returns>
        internal CollectionNotificationChainManager CreateOrGetCollectionManager<T1> (Expression<Func<ObservableCollection<T1>>> collectionPropGetter)
            where T1 : class
        {
            var propName = collectionPropGetter.GetPropertyOrFieldName ();

            NotificationChainManager mgr;
            if (!myDeepChainManagers.TryGetValue (propName, out mgr))
            {
                myDeepChainManagers[propName] = mgr = new CollectionNotificationChainManager ();
                myDeepChainGetters[propName] = _ => collectionPropGetter.Compile ().Invoke ();
                var currentValue = (ObservableCollection<T1>) myDeepChainGetters[propName] (null);
                if (currentValue != null)
                    ((CollectionNotificationChainManager) mgr).ObserveCollection (currentValue);
            }

            return (CollectionNotificationChainManager) mgr;
        }

        /// <summary>
        /// Creates a new NotificationChain for the calling property
        /// </summary>
        /// <param name="dependentPropertyName">Name of the property that depends on other properties</param>
        /// <returns></returns>
        public NotificationChain Get ([CallerMemberName] String dependentPropertyName = null)
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

        /// <summary>
        /// Attempt to determine how to observe the given notifying object, then begin observing it.
        /// </summary>
        /// <param name="notifyingObject"></param>
        public void Observe (Object notifyingObject)
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

        /// <summary>
        /// Begins observing the given notifying object.
        /// </summary>
        /// <param name="notifyingObject"></param>
        public void Observe (INotifyPropertyChanged notifyingObject)
        {
            notifyingObject.ThrowIfNull ("notifyingObject");

            if (IsDisposed) return;

            Observe (notifyingObject, h => notifyingObject.PropertyChanged += h, h => notifyingObject.PropertyChanged -= h);
        }

        /// <summary>
        /// Begins observing the given notifying object.
        /// </summary>
        /// <param name="notifyingObject"></param>
        /// <param name="addEventAction"></param>
        /// <param name="removeEventAction"></param>
        public void Observe (
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

        /// <summary>
        /// Stop observing the given notifying object.
        /// </summary>
        /// <param name="notifyingObject"></param>
        public void StopObserving (Object notifyingObject)
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

        /// <summary>
        /// Pushes PropertyChangedEventArgs input for processing. Normally called by the PropertyChanged event of the current observed object.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns>whether or not the callbacks were triggered</returns>
        public void Publish (Object sender, PropertyChangedEventArgs args)
        {
            sender.ThrowIfNull ("sender");

            if (IsDisposed) return;

            lock (lock_Publish)
            {
                foreach (var chain in myChains.Values)
                    chain.Publish (sender, args);

                NotificationChainManager manager;
                if (!myDeepChainManagers.TryGetValue (args.PropertyName, out manager)) return;

                INotifyPropertyChanged previousPropertyValue = null;
                if (myDeepPreviousObservedValues.ContainsKey (sender))
                    myDeepPreviousObservedValues[sender].TryGetValue (args.PropertyName, out previousPropertyValue);
                var currentPropertyValue = (INotifyPropertyChanged) myDeepChainGetters[args.PropertyName] (sender);

                if (ReferenceEquals (previousPropertyValue, currentPropertyValue)) return;

                if (previousPropertyValue != null)
                {
                    if (manager is CollectionNotificationChainManager)
                        ((CollectionNotificationChainManager) manager).StopObservingCollection ((INotifyCollectionChanged) previousPropertyValue);
                    else
                        manager.StopObserving (previousPropertyValue);
                }

                if (currentPropertyValue != null)
                {
                    if (manager is CollectionNotificationChainManager)
                        ((CollectionNotificationChainManager) manager).ObserveCollection ((INotifyCollectionChanged) currentPropertyValue);
                    else
                        manager.Observe (currentPropertyValue);
                }
            }
        }
    }
}