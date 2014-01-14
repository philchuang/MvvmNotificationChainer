using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;

namespace Com.PhilChuang.Utils.MvvmNotificationChainer
{
    public class ChainedNotification : IDisposable
    {
        public event PropertyChangedEventHandler NotifyingPropertyChanged = delegate { };

        /// <summary>
        /// Name of the property that depends on other properties
        /// </summary>
        public String ChainedPropertyName { get; private set; }

        public bool IsFinished { get; private set; }

        private readonly Dictionary<Object, ChainedNotificationHandler> myNotifierToPropertyNamesMap;
        private readonly PropertyChangedEventHandler myDelegate;

        /// <summary>
        /// </summary>
        /// <param name="chainedPropertyName">Name of the depending property</param>
        public ChainedNotification(String chainedPropertyName)
        {
            chainedPropertyName.ThrowIfNull("chainedPropertyName");

            ChainedPropertyName = chainedPropertyName;

            myNotifierToPropertyNamesMap = new Dictionary<Object, ChainedNotificationHandler>();
            myDelegate = OnNotifyingPropertyChanged;
        }

        public void Dispose()
        {
            foreach (var handler in myNotifierToPropertyNamesMap.Values)
            {
                handler.NotifyingPropertyChanged -= myDelegate;
                handler.Dispose();
            }
            myNotifierToPropertyNamesMap.Clear();
            NotifyingPropertyChanged = null;
        }

        public ChainedNotification Register(Action<ChainedNotification> registrationAction)
        {
            if (IsFinished) return this;

            registrationAction.ThrowIfNull("registrationAction");

            registrationAction(this);

            return this;
        }

        public ChainedNotification On<T>(INotifyPropertyChanged notifyingObject, Expression<Func<T>> propGet)
        {
            if (IsFinished) return this;

            notifyingObject.ThrowIfNull("notifyingObject");
            propGet.ThrowIfNull("propGet");

            return On<T>(notifyingObject, h => notifyingObject.PropertyChanged += h, h => notifyingObject.PropertyChanged -= h, propGet.GetPropertyName());
        }

        public ChainedNotification On<T>(Object notifyingObject, Action<PropertyChangedEventHandler> addEventAction, Action<PropertyChangedEventHandler> removeEventAction, Expression<Func<T>> propGet)
        {
            if (IsFinished) return this;

            addEventAction.ThrowIfNull("addEventAction");
            removeEventAction.ThrowIfNull("removeEventAction");
            propGet.ThrowIfNull("propGet");

            return On<T>(notifyingObject, addEventAction, removeEventAction, propGet.GetPropertyName());
        }

        public ChainedNotification On<T>(Object notifyingObject, Action<PropertyChangedEventHandler> addEventAction, Action<PropertyChangedEventHandler> removeEventAction, String propertyName)
        {
            if (IsFinished) return this;

            addEventAction.ThrowIfNull("addEventAction");
            removeEventAction.ThrowIfNull("removeEventAction");
            propertyName.ThrowIfNullOrBlank("propertyName");

            ChainedNotificationHandler handler;
            if (!myNotifierToPropertyNamesMap.TryGetValue(notifyingObject, out handler))
            {
                handler = myNotifierToPropertyNamesMap[notifyingObject] = new ChainedNotificationHandler(addEventAction, removeEventAction);
                handler.NotifyingPropertyChanged += myDelegate;
            }

            handler.NotifyingPropertyNames.Add(propertyName);

            return this;
        }

        public ChainedNotification AndCall(Action<String> onNotifyingPropertyChanged)
        {
            if (IsFinished) return this;

            onNotifyingPropertyChanged.ThrowIfNull("onNotifyingPropertyChanged");

            NotifyingPropertyChanged += (sender, args) => onNotifyingPropertyChanged (ChainedPropertyName);

            return this;
        }

        public void Finish()
        {
            if (IsFinished) return;

            IsFinished = true;
        }

        private void OnNotifyingPropertyChanged(Object sender, PropertyChangedEventArgs args)
        {
            var handler = NotifyingPropertyChanged;
            handler(sender, args);
        }
    }
}
