using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Com.PhilChuang.Utils.MvvmNotificationChainer
{
    /// <summary>
    /// Manages multiple NotificationChains for a single notifying collection.
    /// Prevents duplication of NotificationChains by dependent property name.
    /// When disposing, calls Dispose on all NotificationChains.
    /// </summary>
    public class CollectionNotificationChainManager : NotificationChainManager
    {
        /// <summary>
        /// The proeprty name to use for the collection being observed by this CollectionNotificationChainManager
        /// </summary>
        public const String ObservedCollectionPropertyName = ".";

        /// <summary>
        /// Map of an observed collection to the delegate to remove the handler for it
        /// </summary>
        private Dictionary<INotifyCollectionChanged, Action> myObservedCollections = new Dictionary<INotifyCollectionChanged, Action> ();

        public IEnumerable<INotifyCollectionChanged> ObservedCollections { get { return myObservedCollections.Keys; } }

        private NotifyCollectionChangedEventHandler myCollectionChangedEventHandler;

        public CollectionNotificationChainManager ()
        {
            myCollectionChangedEventHandler = OnCollectionChanged;
        }

        public CollectionNotificationChainManager (INotifyCollectionChanged notifyingCollection)
            : this ()
        {
            notifyingCollection.ThrowIfNull ("notifyingCollection");

            ObserveCollection (notifyingCollection);
        }

        public override void Dispose ()
        {
            if (IsDisposed) return;

            foreach (var kvp in myObservedCollections)
                kvp.Value ();
            myObservedCollections.Clear();
            myObservedCollections = null;

            myCollectionChangedEventHandler = null;

            base.Dispose ();
        }

        /// <summary>
        /// Begins observing the given notifying collection.
        /// </summary>
        /// <param name="notifyingCollection"></param>
        public void ObserveCollection (INotifyCollectionChanged notifyingCollection)
        {
            notifyingCollection.ThrowIfNull ("notifyingCollection");

            if (IsDisposed) return;

            if (myObservedCollections.ContainsKey (notifyingCollection)) return;

            myObservedCollections[notifyingCollection] = () => notifyingCollection.CollectionChanged -= myCollectionChangedEventHandler;

            NotificationChainPropertyAttribute.CallProperties (notifyingCollection);

            notifyingCollection.CollectionChanged += myCollectionChangedEventHandler;

            if (notifyingCollection is IEnumerable)
                foreach (var item in (IEnumerable) notifyingCollection)
                    base.Observe (item);
        }

        /// <summary>
        /// Stop observing the given notifying collection.
        /// </summary>
        /// <param name="notifyingCollection"></param>
        public void StopObservingCollection (INotifyCollectionChanged notifyingCollection)
        {
            notifyingCollection.ThrowIfNull ("notifyingCollection");

            if (IsDisposed) return;

            Action removeHandler;
            if (!myObservedCollections.TryGetValue (notifyingCollection, out removeHandler))
                return;

            lock (this)
            {
                removeHandler ();

                if (notifyingCollection is IEnumerable)
                    foreach (var item in (IEnumerable) notifyingCollection)
                        base.StopObserving (item);

                myObservedCollections.Remove (notifyingCollection);
            }
        }

        private void OnCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                foreach (var oldItem in e.OldItems)
                    base.StopObserving (oldItem);
            if (e.NewItems != null)
                foreach (var newItem in e.NewItems)
                    base.Observe (newItem);

            Publish (sender, new PropertyChangedEventArgs (ObservedCollectionPropertyName));
        }
    }
}