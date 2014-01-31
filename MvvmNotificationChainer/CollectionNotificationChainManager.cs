using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Com.PhilChuang.Utils.MvvmNotificationChainer
{
    /// <summary>
    /// Manages multiple NotificationChains for a single notifying collection.
    /// Prevents duplication of NotificationChains by dependent property name.
    /// When disposing, calls Dispose on all NotificationChains.
    /// </summary>
    public class CollectionNotificationChainManager : NotificationChainManager
    {
        public INotifyCollectionChanged ObservedCollection { get; private set; }

        private NotifyCollectionChangedEventHandler myCollectionChangedEventHandler;

        private Action<NotifyCollectionChangedEventHandler> myRemoveCollectionChangedEventHandler;

        public CollectionNotificationChainManager ()
        {
            myCollectionChangedEventHandler = OnCollectionChanged;
        }

        public CollectionNotificationChainManager (INotifyCollectionChanged notifyingObject)
            : this ()
        {
            notifyingObject.ThrowIfNull ("INotifyPropertyChanged");

            Observe (notifyingObject);
        }

        public override void Dispose ()
        {
            if (IsDisposed) return;

            if (myRemoveCollectionChangedEventHandler != null)
            {
                myRemoveCollectionChangedEventHandler (myCollectionChangedEventHandler);
                myRemoveCollectionChangedEventHandler = null;
            }
            myCollectionChangedEventHandler = null;

            base.Dispose ();
        }

        public override void Observe (Object notifyingCollection)
        {
            notifyingCollection.ThrowIfNull ("notifyingCollection");

            if (IsDisposed) return;

            if (notifyingCollection is INotifyCollectionChanged)
                Observe ((INotifyCollectionChanged) notifyingCollection);

            throw new InvalidOperationException ("Unable to observe an object of type \"{0}\"".FormatWith (notifyingCollection.GetType ().FullName));
        }

        public void Observe (INotifyCollectionChanged notifyingCollection)
        {
            notifyingCollection.ThrowIfNull ("notifyingCollection");

            if (IsDisposed) return;

            if (ReferenceEquals (ObservedCollection, notifyingCollection)) return;

            if (ObservedCollection != null)
                throw new InvalidOperationException ("Can't observe a different collection without calling StopObserving() first");

            ObservedCollection = notifyingCollection;

            NotificationChainPropertyAttribute.CallProperties (ObservedCollection);

            notifyingCollection.CollectionChanged += myCollectionChangedEventHandler;
            myRemoveCollectionChangedEventHandler = h => notifyingCollection.CollectionChanged -= h;
        }

        public override void Observe (INotifyPropertyChanged notifyingObject)
        {
            throw new InvalidOperationException ("Cannot call Observe (INotifyPropertyChanged notifyingObject) on this class");
        }

        public override void Observe (
            Object notifyingObject,
            Action<PropertyChangedEventHandler> addEventAction,
            Action<PropertyChangedEventHandler> removeEventAction)
        {
            throw new InvalidOperationException ("Cannot call Observe (Object notifyingObject, Action<PropertyChangedEventHandler> addEventAction, Action<PropertyChangedEventHandler> removeEventAction) on this class");
        }

        public override void StopObserving (object notifyingCollection)
        {
            notifyingCollection.ThrowIfNull ("notifyingCollection");

            if (!ReferenceEquals (ObservedCollection, notifyingCollection))
                throw new ArgumentException("The given notifying collection does not match the current collection");

            if (ObservedCollection == null) return;

            lock (this)
            {
                ObservedCollection = null;
                myRemoveCollectionChangedEventHandler (myCollectionChangedEventHandler);
                myRemoveCollectionChangedEventHandler = null;
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
        }
    }
}