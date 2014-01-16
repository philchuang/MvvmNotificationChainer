using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;

namespace Com.PhilChuang.Utils.MvvmNotificationChainer
{
    /// <summary>
    /// Defines a ChainedNotification. Observes multiple properties on multiple objects.
    /// </summary>
    public class ChainedNotification : IDisposable
    {
        /// <summary>
        /// Fires when an observed property is changed. Can listen to this event directly or call AndCall().
        /// </summary>
        public event PropertyChangedEventHandler NotifyingPropertyChanged = delegate { };

        /// <summary>
        /// Name of the property that depends on other properties (e.g. Cost depends on Quantity and Price)
        /// </summary>
        public String ChainedPropertyName { get; private set; }

        /// <summary>
        /// Whether or not the notification has been fully defined (if false, then modifications are still allowed)
        /// </summary>
        public bool IsFinished { get; private set; }

		private object DefaultNotifyingObject { get; set; }
		private Action<PropertyChangedEventHandler> DefaultAddEventAction { get; set; }
		private Action<PropertyChangedEventHandler> DefaultRemoveEventAction { get; set; }

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

        private void OnNotifyingPropertyChanged(Object sender, PropertyChangedEventArgs args)
        {
            var handler = NotifyingPropertyChanged;
            handler(sender, args);
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

        public ChainedNotification AndSetDefaultNotifier(Object notifyingObject, Action<PropertyChangedEventHandler> addEventAction, Action<PropertyChangedEventHandler> removeEventAction)
        {
            DefaultNotifyingObject = notifyingObject;
            DefaultAddEventAction = addEventAction;
            DefaultRemoveEventAction = removeEventAction;
            return this;
        }

        /// <summary>
        /// Performs the registration action on the current ChainedNotification (if not yet finished).
        /// </summary>
        /// <param name="registrationAction"></param>
        /// <returns></returns>
        public ChainedNotification Register(Action<ChainedNotification> registrationAction)
        {
            if (IsFinished) return this;

            registrationAction.ThrowIfNull("registrationAction");

            registrationAction(this);

            return this;
        }

        /// <summary>
        /// Specifies property name to observe on the default notifying object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propGet"></param>
        /// <returns></returns>
        public ChainedNotification On<T>(Expression<Func<T>> propGet)
        {
	        return On<T> (DefaultNotifyingObject, DefaultAddEventAction, DefaultRemoveEventAction, propGet.GetPropertyName ());
        }

        /// <summary>
        /// Specifies property name to observe on the default notifying object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
		/// <param name="propertyName"></param>
        /// <returns></returns>
        public ChainedNotification On<T>(String propertyName)
        {
			return On<T> (DefaultNotifyingObject, DefaultAddEventAction, DefaultRemoveEventAction, propertyName);
        }

        /// <summary>
        /// Specifies an INotifyPropertyChanged object and property name to observe.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="notifyingObject"></param>
        /// <param name="propGet"></param>
        /// <returns></returns>
        public ChainedNotification On<T>(INotifyPropertyChanged notifyingObject, Expression<Func<T>> propGet)
        {
            if (IsFinished) return this;

            notifyingObject.ThrowIfNull("notifyingObject");
            propGet.ThrowIfNull("propGet");

            return On<T>(notifyingObject, h => notifyingObject.PropertyChanged += h, h => notifyingObject.PropertyChanged -= h, propGet.GetPropertyName());
        }

        /// <summary>
        /// Specifies a PropertyChangedEvent and property name to observe.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="notifyingObject"></param>
        /// <param name="addEventAction"></param>
        /// <param name="removeEventAction"></param>
        /// <param name="propGet"></param>
        /// <returns></returns>
        public ChainedNotification On<T>(Object notifyingObject, Action<PropertyChangedEventHandler> addEventAction, Action<PropertyChangedEventHandler> removeEventAction, Expression<Func<T>> propGet)
        {
            if (IsFinished) return this;

            addEventAction.ThrowIfNull("addEventAction");
            removeEventAction.ThrowIfNull("removeEventAction");
            propGet.ThrowIfNull("propGet");

            return On<T>(notifyingObject, addEventAction, removeEventAction, propGet.GetPropertyName());
        }

        /// <summary>
        /// Specifies a PropertyChangedEvent and property name to observe.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="notifyingObject"></param>
        /// <param name="addEventAction"></param>
        /// <param name="removeEventAction"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Specifies an action to invoke when a notifying property is changed. Multiple actions can be invoked.
        /// </summary>
        /// <param name="onNotifyingPropertyChanged"></param>
        /// <returns></returns>
        public ChainedNotification AndCall(Action<String> onNotifyingPropertyChanged)
        {
            if (IsFinished) return this;

            onNotifyingPropertyChanged.ThrowIfNull("onNotifyingPropertyChanged");

            NotifyingPropertyChanged += (sender, args) => onNotifyingPropertyChanged (ChainedPropertyName);

            return this;
        }

        /// <summary>
        /// Indicates that the ChainedNotification has been fully defined and prevents further modification/registration.
        /// </summary>
        public void Finish()
        {
            if (IsFinished) return;

            IsFinished = true;
        }
	}
}
