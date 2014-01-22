using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Com.PhilChuang.Utils.MvvmNotificationChainer
{
    public class ObserverWrapper : IDisposable
    {
        private Object myNotifyingObject;
        private Action<PropertyChangedEventHandler> myAddEventAction;
        private Action<PropertyChangedEventHandler> myRemoveEventAction;
        private bool myIsDisposed;

        public ObserverWrapper (INotifyPropertyChanged notifyingObject)
        {
            notifyingObject.ThrowIfNull("notifyingObject");

            myNotifyingObject = notifyingObject;
            myAddEventAction = h => notifyingObject.PropertyChanged += h;
            myRemoveEventAction = h => notifyingObject.PropertyChanged -= h;
        }

        public ObserverWrapper (Object notifyingObject,
                                Action<PropertyChangedEventHandler> addEventAction,
                                Action<PropertyChangedEventHandler> removeEventAction)
        {
            notifyingObject.ThrowIfNull("notifyingObject");
            addEventAction.ThrowIfNull("addEventAction");
            removeEventAction.ThrowIfNull("removeEventAction");

            myNotifyingObject = notifyingObject;
            myAddEventAction = addEventAction;
            myRemoveEventAction = removeEventAction;
        }

        public void Dispose ()
        {
            if (myIsDisposed) return;

            myNotifyingObject = null;
            myAddEventAction = null;
            myRemoveEventAction = null;

            myIsDisposed = true;
        }
    }
}
