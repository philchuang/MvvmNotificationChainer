using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Com.PhilChuang.Utils.MvvmNotificationChainer
{
    public interface INotificationChainManagerWrapper
    {
        Object Wrapped { get; }
        NotificationChainManager Manager { get; }
    }

    public class NotificationChainManagerWrapper<T> : INotificationChainManagerWrapper, IDisposable
        where T : class
    {
        public Object Wrapped { get { return WrappedTyped; } }
        public T WrappedTyped { get; private set; }
        public NotificationChainManager Manager { get; private set; }

        public NotificationChainManagerWrapper (INotifyPropertyChanged notifyingObject)
        {
            notifyingObject.ThrowIfNull ("notifyingObject");

            Initialize ((T) notifyingObject, h => notifyingObject.PropertyChanged += h, h => notifyingObject.PropertyChanged -= h);
        }

        public NotificationChainManagerWrapper (T notifyingObject, Action<PropertyChangedEventHandler> addEventAction, Action<PropertyChangedEventHandler> removeEventAction)
        {
            notifyingObject.ThrowIfNull ("notifyingObject");

            Initialize (notifyingObject, addEventAction, removeEventAction);
        }

        private void Initialize (T notifyingObject, Action<PropertyChangedEventHandler> addEventAction, Action<PropertyChangedEventHandler> removeEventAction)
        {
            WrappedTyped = notifyingObject;
            
            Manager = new NotificationChainManager();
            Manager.SetDefaultNotifyingObject (notifyingObject, addEventAction, removeEventAction);
        }

        public void Dispose ()
        {
            Manager.Dispose();
            Manager = null;
            WrappedTyped = null;
        }
    }
}
