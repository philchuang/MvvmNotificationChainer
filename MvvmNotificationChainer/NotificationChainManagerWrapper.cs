using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;

namespace Com.PhilChuang.Utils.MvvmNotificationChainer
{
    public class NotificationChainManagerWrapper<T> : INotificationChainManager<T>
        where T : class
    {
        public Object ObservedObject { get { return Observed; } }

        public T Observed { get { return Manager != null ? Manager.Observed : null; } }

        public bool IsDisposed { get; private set; }

        public NotificationChainManager<T> Manager { get; private set; }

        public NotificationChainManagerWrapper ()
        {
            Manager = new NotificationChainManager<T>();
        }

        public NotificationChainManagerWrapper (INotifyPropertyChanged notifyingObject) : this ()
        {
            notifyingObject.ThrowIfNull ("INotifyPropertyChanged");
            if (!(notifyingObject is T))
                throw new ArgumentException ("Expected type {0}, got {1}".FormatWith (typeof (T).Name, notifyingObject.GetType ().Name));

            Observe (notifyingObject);
        }

        public NotificationChainManagerWrapper (
            T notifyingObject,
            Action<PropertyChangedEventHandler> addEventAction,
            Action<PropertyChangedEventHandler> removeEventAction) : this ()
        {
            Observe (notifyingObject, addEventAction, removeEventAction);
        }

        public void Dispose ()
        {
            if (IsDisposed) return;

            Manager.Dispose ();
            Manager = null;

            IsDisposed = true;
        }

        public void AddDefaultCall (Action onNotifyingPropertyChanged)
        {
            if (IsDisposed) return;

            Manager.AddDefaultCall (onNotifyingPropertyChanged);
        }

        public void AddDefaultCall (NotificationChainCallback onNotifyingPropertyChanged)
        {
            if (IsDisposed) return;

            Manager.AddDefaultCall (onNotifyingPropertyChanged);
        }

        public NotificationChain CreateOrGet<T1> (Expression<Func<T1>> propGetter)
        {
            if (IsDisposed) throw new ObjectDisposedException ("Manager");

            return Manager.CreateOrGet (propGetter);
        }

        public NotificationChain CreateOrGet ([CallerMemberName] string dependentPropertyName = null)
        {
            if (IsDisposed) throw new ObjectDisposedException ("Manager");

            return Manager.CreateOrGet (dependentPropertyName);
        }

        public INotificationChainManager<T1> CreateOrGetManager<T1> (Expression<Func<T1>> propGetter)
            where T1 : class
        {
            if (IsDisposed) throw new ObjectDisposedException ("Manager");

            return Manager.CreateOrGetManager (propGetter);
        }

        public INotificationChainManager<T1> CreateOrGetManager<T0, T1> (Expression<Func<T0, T1>> propGetter)
            where T0 : T
            where T1 : class
        {
            if (IsDisposed) throw new ObjectDisposedException ("Manager");

            return Manager.CreateOrGetManager (propGetter);
        }


        public NotificationChain Get ([CallerMemberName] string dependentPropertyName = null)
        {
            if (IsDisposed) throw new ObjectDisposedException ("Manager");

            return Manager.Get (dependentPropertyName);
        }

        public void Clear ([CallerMemberName] string dependentPropertyName = null)
        {
            if (IsDisposed) return;

            Manager.Clear (dependentPropertyName);
        }

        public void Observe<T0> (T0 notifyingObject)
            where T0 : T, INotifyPropertyChanged
        {
            notifyingObject.ThrowIfNull ("notifyingObject");

            if (IsDisposed) return;

            Observe ((INotifyPropertyChanged) notifyingObject);
        }

        public void Observe (INotifyPropertyChanged notifyingObject)
        {
            notifyingObject.ThrowIfNull ("notifyingObject");
            if (!(notifyingObject is T))
                throw new ArgumentException ("Expected type {0}, got {1}".FormatWith (typeof (T).Name, notifyingObject.GetType ().Name));

            if (IsDisposed) return;
            
            Manager.Observe (notifyingObject);
        }

        public void Observe (T notifyingObject,
                             Action<PropertyChangedEventHandler> addEventAction,
                             Action<PropertyChangedEventHandler> removeEventAction)
        {
            if (IsDisposed) return;

            Observe ((Object) notifyingObject, addEventAction, removeEventAction);
        }

        public void Observe (object notifyingObject, Action<PropertyChangedEventHandler> addEventAction, Action<PropertyChangedEventHandler> removeEventAction)
        {
            if (IsDisposed) return;

            Manager.Observe (notifyingObject, addEventAction, removeEventAction);
        }

        public void StopObserving ()
        {
            if (IsDisposed) return;

            Manager.StopObserving ();
        }

        public void Publish (object sender, PropertyChangedEventArgs args)
        {
            if (IsDisposed) return;

            Manager.Publish (sender, args);
        }

        public void Publish (T sender, PropertyChangedEventArgs args)
        {
            if (IsDisposed) return;

            Manager.Publish (sender, args);
        }
    }
}
