using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Com.PhilChuang.Utils.MvvmNotificationChainer
{
    /// <summary>
    /// Used internally by ChainedNotification. Observes multiple properties on a single object.
    /// </summary>
    internal class ChainedNotificationHandler : IDisposable
    {
        public event PropertyChangedEventHandler NotifyingPropertyChanged = delegate { };

        public List<String> NotifyingPropertyNames { get; private set; }

        private readonly PropertyChangedEventHandler myDelegate;
        private Action<PropertyChangedEventHandler> myRemoveEventHandlerAction;

        public ChainedNotificationHandler(INotifyPropertyChanged notifyingObject)
        {
            notifyingObject.ThrowIfNull("notifyingObject");

            NotifyingPropertyNames = new List<string>();
            myDelegate = OnPropertyChanged;

            notifyingObject.PropertyChanged += myDelegate;
            myRemoveEventHandlerAction = h => notifyingObject.PropertyChanged -= h;
        }

        public ChainedNotificationHandler(Action<PropertyChangedEventHandler> addEventHandlerAction, Action<PropertyChangedEventHandler> removeEventHandlerAction)
        {
            addEventHandlerAction.ThrowIfNull("addEventHandlerAction");
            removeEventHandlerAction.ThrowIfNull("removeEventHandlerAction");

            NotifyingPropertyNames = new List<string>();
            myDelegate = OnPropertyChanged;

            addEventHandlerAction(myDelegate);
            myRemoveEventHandlerAction = removeEventHandlerAction;
        }

        public void Dispose()
        {
            if (myRemoveEventHandlerAction != null)
            {
                myRemoveEventHandlerAction (myDelegate);
                myRemoveEventHandlerAction = null;
                NotifyingPropertyNames = null;
                NotifyingPropertyChanged = null;
            }
        }

        private void OnPropertyChanged(Object sender, PropertyChangedEventArgs args)
        {
            if (NotifyingPropertyNames.Contains (args.PropertyName))
            {
                var handler = NotifyingPropertyChanged;
                handler (sender, args);
            }
        }
    }
}
