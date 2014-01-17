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
    /// Defines a ChainedNotification. Observes multiple notifying properties on multiple objects and triggers NotifyingPropertyChanged for the dependent property.
    /// </summary>
    public class ChainedNotification : IDisposable
    {
        /// <summary>
        /// Fires when an observed property is changed. Can listen to this event directly or call AndCall().
        /// First String parameter is the notifying property. Second String parameter is the dependent property.
        /// </summary>
        public event Action<String, String> NotifyingPropertyChanged = delegate { };

        /// <summary>
        /// Name of the property that depends on other properties (e.g. Cost depends on Quantity and Price)
        /// </summary>
        public String DependentPropertyName { get; private set; }

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
        /// <param name="dependentPropertyName">Name of the depending property</param>
        public ChainedNotification (String dependentPropertyName)
        {
            dependentPropertyName.ThrowIfNull ("dependentPropertyName");

            DependentPropertyName = dependentPropertyName;

            myNotifierToPropertyNamesMap = new Dictionary<Object, ChainedNotificationHandler> ();
            myDelegate = OnNotifyingPropertyChanged;
        }

        private void OnNotifyingPropertyChanged (Object sender, PropertyChangedEventArgs args)
        {
            var handler = NotifyingPropertyChanged;
            handler (args.PropertyName, DependentPropertyName);
        }

        public void Dispose ()
        {
            foreach (var handler in myNotifierToPropertyNamesMap.Values)
            {
                handler.NotifyingPropertyChanged -= myDelegate;
                handler.Dispose ();
            }
            myNotifierToPropertyNamesMap.Clear ();
            NotifyingPropertyChanged = null;
        }

        public ChainedNotification AndSetDefaultNotifyingObject (INotifyPropertyChanged notifyingObject)
        {
            return AndSetDefaultNotifyingObject (notifyingObject, h => notifyingObject.PropertyChanged += h, h => notifyingObject.PropertyChanged -= h);
        }

        public ChainedNotification AndSetDefaultNotifyingObject (Object notifyingObject, Action<PropertyChangedEventHandler> addEventAction, Action<PropertyChangedEventHandler> removeEventAction)
        {
            DefaultNotifyingObject = notifyingObject;
            DefaultAddEventAction = addEventAction;
            DefaultRemoveEventAction = removeEventAction;
            return this;
        }

        /// <summary>
        /// Performs the registration/setup action on the current ChainedNotification (if not yet finished).
        /// </summary>
        /// <param name="registrationAction"></param>
        /// <returns></returns>
        public ChainedNotification Register (Action<ChainedNotification> registrationAction)
        {
            if (IsFinished) return this;

            registrationAction.ThrowIfNull ("registrationAction");

            registrationAction (this);

            return this;
        }

        /// <summary>
        /// Specifies property name to observe on the default notifying object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propGetter"></param>
        /// <returns></returns>
        public ChainedNotification On<T> (Expression<Func<T>> propGetter)
        {
            return On (DefaultNotifyingObject, DefaultAddEventAction, DefaultRemoveEventAction, propGetter.GetPropertyName ());
        }

        /// <summary>
        /// Specifies property name to observe on the default notifying object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public ChainedNotification On (String propertyName)
        {
            return On (DefaultNotifyingObject, DefaultAddEventAction, DefaultRemoveEventAction, propertyName);
        }

        /// <summary>
        /// Specifies an INotifyPropertyChanged object and property name to observe.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="notifyingObject"></param>
        /// <param name="propGetter"></param>
        /// <returns></returns>
        public ChainedNotification On<T> (INotifyPropertyChanged notifyingObject, Expression<Func<T>> propGetter)
        {
            if (IsFinished) return this;

            notifyingObject.ThrowIfNull ("notifyingObject");
            propGetter.ThrowIfNull ("propGetter");

            return On (notifyingObject, h => notifyingObject.PropertyChanged += h, h => notifyingObject.PropertyChanged -= h, propGetter.GetPropertyName ());
        }

        /// <summary>
        /// Specifies a PropertyChangedEvent and property name to observe.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="notifyingObject"></param>
        /// <param name="addEventAction"></param>
        /// <param name="removeEventAction"></param>
        /// <param name="propGetter"></param>
        /// <returns></returns>
        public ChainedNotification On<T> (
            Object notifyingObject, 
            Action<PropertyChangedEventHandler> addEventAction, 
            Action<PropertyChangedEventHandler> removeEventAction, 
            Expression<Func<T>> propGetter)
        {
            if (IsFinished) return this;

            addEventAction.ThrowIfNull ("addEventAction");
            removeEventAction.ThrowIfNull ("removeEventAction");
            propGetter.ThrowIfNull ("propGetter");

            return On (notifyingObject, addEventAction, removeEventAction, propGetter.GetPropertyName ());
        }

        /// <summary>
        /// Specifies a PropertyChangedEvent and property name to observe.
        /// </summary>
        /// <param name="notifyingObject"></param>
        /// <param name="addEventAction"></param>
        /// <param name="removeEventAction"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public ChainedNotification On (
            Object notifyingObject, 
            Action<PropertyChangedEventHandler> addEventAction, 
            Action<PropertyChangedEventHandler> removeEventAction, 
            String propertyName)
        {
            if (IsFinished) return this;

            addEventAction.ThrowIfNull ("addEventAction");
            removeEventAction.ThrowIfNull ("removeEventAction");
            propertyName.ThrowIfNullOrBlank ("propertyName");

            ChainedNotificationHandler handler;
            if (!myNotifierToPropertyNamesMap.TryGetValue (notifyingObject, out handler))
            {
                handler = myNotifierToPropertyNamesMap[notifyingObject] = new ChainedNotificationHandler (addEventAction, removeEventAction);
                handler.NotifyingPropertyChanged += myDelegate;
            }

            if (!handler.NotifyingPropertyNames.Contains (propertyName))
                handler.NotifyingPropertyNames.Add (propertyName);

            return this;
        }

        /// <summary>
        /// Specifies a property of type INotifyPropertyChanged to observe on the default notifying object, and sub-property to observe
        /// </summary>
        /// <typeparam name="T1">The top-level Property to observe on notifyingObject, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T2">The sub Property to observe on T1</typeparam>
        /// <param name="propGetter"></param>
        /// <param name="subPropGetter"></param>
        /// <returns></returns>
        public ChainedNotification On<T1, T2> (
            Expression<Func<T1>> propGetter,
            Expression<Func<T1, T2>> subPropGetter)
            where T1 : INotifyPropertyChanged
        {
            if (IsFinished) return this;

            return On (DefaultNotifyingObject, DefaultAddEventAction, DefaultRemoveEventAction, propGetter, subPropGetter);
        }

        /// <summary>
        /// Specifies a notifying object, property of type INotifyPropertyChanged, and sub-property to observe
        /// </summary>
        /// <typeparam name="T1">The top-level Property to observe on notifyingObject, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T2">The sub Property to observe on T1</typeparam>
        /// <param name="notifyingObject"></param>
        /// <param name="addEventAction"></param>
        /// <param name="removeEventAction"></param>
        /// <param name="propGetter"></param>
        /// <param name="subPropGetter"></param>
        /// <returns></returns>
        public ChainedNotification On<T1, T2> (
            Object notifyingObject,
            Action<PropertyChangedEventHandler> addEventAction,
            Action<PropertyChangedEventHandler> removeEventAction,
            Expression<Func<T1>> propGetter,
            Expression<Func<T1, T2>> subPropGetter)
            where T1 : INotifyPropertyChanged
        {
            if (IsFinished) return this;

            /* How's this going to work?
             * 1) register handler on notifying object, looking for propGetter
             * 2) when property notifies, evaluate it
             * 2.1) if it is non-null and has not been chained, then chain it to subPropGet
             * 2.2) if it is non-null and has already been chained, do nothing
             * 2.3) if it is null and has not been chained, do nothing
             * 2.4) if it is null and has already been chained, dispose the chain
             */
            // need to create a new kind of handler
            // - only handles a single property
            // - can get and retain the property value
            throw new NotImplementedException ();
            return this;
        }

        /// <summary>
        /// Specifies an action to invoke when a notifying property is changed. Multiple actions can be invoked.
        /// </summary>
        /// <param name="onNotifyingPropertyChanged">First String parameter is the notifying property. Second String parameter is the chained property</param>
        /// <returns></returns>
        public ChainedNotification AndCall (Action<String, String> onNotifyingPropertyChanged)
        {
            if (IsFinished) return this;

            onNotifyingPropertyChanged.ThrowIfNull ("onNotifyingPropertyChanged");

            NotifyingPropertyChanged += onNotifyingPropertyChanged;

            return this;
        }

        /// <summary>
        /// Indicates that the ChainedNotification has been fully defined and prevents further modification/registration.
        /// </summary>
        public void Finish ()
        {
            if (IsFinished) return;

            IsFinished = true;
        }
    }
}