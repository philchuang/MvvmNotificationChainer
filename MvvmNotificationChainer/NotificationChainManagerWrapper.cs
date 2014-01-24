using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;

namespace Com.PhilChuang.Utils.MvvmNotificationChainer
{
    public class NotificationChainManagerWrapper<T> : INotificationChainManager
    {
        public object ObservedObject { get { return Manager != null ? Manager.ObservedObject : null; } }

        public T Wrapped { get { return Manager != null ? (T) Manager.ObservedObject : default(T); } }

        public bool IsDisposed { get; private set; }

        public NotificationChainManager Manager { get; private set; }

        public NotificationChainManagerWrapper ()
        {
            Manager = new NotificationChainManager();
        }

        public NotificationChainManagerWrapper (INotifyPropertyChanged notifyingObject) : this ()
        {
            Observe (notifyingObject);
        }

        public NotificationChainManagerWrapper (
            Object notifyingObject,
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
        { Manager.AddDefaultCall (onNotifyingPropertyChanged); }

        public void AddDefaultCall (NotificationChainCallback onNotifyingPropertyChanged)
        { Manager.AddDefaultCall (onNotifyingPropertyChanged); }

        public NotificationChain CreateOrGet<T1> (Expression<Func<T1>> propGetter)
        { return Manager.CreateOrGet (propGetter); }

        public NotificationChain CreateOrGet ([CallerMemberName] string dependentPropertyName = null)
        { return Manager.CreateOrGet (dependentPropertyName); }

        public NotificationChain Get ([CallerMemberName] string dependentPropertyName = null)
        { return Manager.Get (dependentPropertyName); }

        public void Clear ([CallerMemberName] string dependentPropertyName = null)
        { Manager.Clear (dependentPropertyName); }

        public void Observe<TInpc> (TInpc notifyingObject)
            where TInpc : T, INotifyPropertyChanged
        { Observe ((INotifyPropertyChanged) notifyingObject); }

        public void Observe (INotifyPropertyChanged notifyingObject)
        { Manager.Observe (notifyingObject); }

        public void Observe (object notifyingObject, Action<PropertyChangedEventHandler> addEventAction, Action<PropertyChangedEventHandler> removeEventAction)
        { Manager.Observe (notifyingObject, addEventAction, removeEventAction); }

        public void StopObserving ()
        { Manager.StopObserving(); }

        public void Publish (object sender, PropertyChangedEventArgs args)
        { Manager.Publish (sender, args); }
    }
}
