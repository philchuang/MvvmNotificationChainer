using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

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

        private readonly Dictionary<Object, NotifyingPropertiesObserver> myNotifierToObserverMap;
        private readonly List<NotifyingPropertiesObserver> myOtherObservers = new List<NotifyingPropertiesObserver> ();
        private readonly PropertyChangedEventHandler myDelegate;

        /// <summary>
        /// </summary>
        /// <param name="dependentPropertyName">Name of the depending property</param>
        public ChainedNotification (String dependentPropertyName)
        {
            dependentPropertyName.ThrowIfNull ("dependentPropertyName");

            DependentPropertyName = dependentPropertyName;

            myNotifierToObserverMap = new Dictionary<Object, NotifyingPropertiesObserver> ();
            myDelegate = OnNotifyingPropertyChanged;
        }

        private void OnNotifyingPropertyChanged (Object sender, PropertyChangedEventArgs args)
        {
            var handler = NotifyingPropertyChanged;
            handler (args.PropertyName, DependentPropertyName);
        }

        public void Dispose ()
        {
            foreach (var observer in myNotifierToObserverMap.Values.Union (myOtherObservers))
            {
                observer.NotifyingPropertyChanged -= myDelegate;
                observer.Dispose ();
            }
            myNotifierToObserverMap.Clear ();
            myOtherObservers.Clear();
            NotifyingPropertyChanged = null;
        }

        public ChainedNotification AndSetDefaultNotifyingObject (INotifyPropertyChanged notifyingObject)
        {
            return AndSetDefaultNotifyingObject (notifyingObject, h => notifyingObject.PropertyChanged += h, h => notifyingObject.PropertyChanged -= h);
        }

        public ChainedNotification AndSetDefaultNotifyingObject (Object notifyingObject,
                                                                 Action<PropertyChangedEventHandler> addEventAction,
                                                                 Action<PropertyChangedEventHandler> removeEventAction)
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
        /// Specifies an INotifyPropertyChanged object and property name to observe.
        /// </summary>
        /// <param name="notifyingObject"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public ChainedNotification On (INotifyPropertyChanged notifyingObject, string propertyName)
        {
            if (IsFinished) return this;

            notifyingObject.ThrowIfNull ("notifyingObject");
            propertyName.ThrowIfNullOrBlank ("propertyName");

            return On (notifyingObject, h => notifyingObject.PropertyChanged += h, h => notifyingObject.PropertyChanged -= h, propertyName);
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

            CreateOrGetObserver (notifyingObject, addEventAction, removeEventAction, propertyName);

            return this;
        }

        private NotifyingPropertiesObserver CreateOrGetObserver (object notifyingObject, Action<PropertyChangedEventHandler> addEventAction, Action<PropertyChangedEventHandler> removeEventAction, string propertyName)
        {
            NotifyingPropertiesObserver observer;
            if (!myNotifierToObserverMap.TryGetValue (notifyingObject, out observer))
            {
                observer = myNotifierToObserverMap[notifyingObject] = new NotifyingPropertiesObserver (addEventAction, removeEventAction);
                observer.NotifyingPropertyChanged += myDelegate;
            }

            if (!observer.NotifyingPropertyNames.Contains (propertyName))
                observer.NotifyingPropertyNames.Add (propertyName);

            return observer;
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
            where T1 : class, INotifyPropertyChanged
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
            where T1 : class, INotifyPropertyChanged
        {
            if (IsFinished) return this;

            // TODO think about how to make this go a few levels deeper!

            /* How this works
             * 1) create new observer on notifying object, looking for propGetter
             * 2) when property notifies, evaluate it
             * 2.1) if it is non-null and has not been observed, then start observing subPropGet
             * 2.2) if it is non-null and has already been observed, do nothing
             * 2.3) if it is null and has not been observed, do nothing
             * 2.4) if it is null and has already been observed, dispose the observer
             */

            // chain Parent.Property to this.DependentPropertyName
            // have to create a separate observer despite having same parent notifying object because each one will behave differently
            var parentPropertyObserver = new NotifyingPropertiesObserver (addEventAction, removeEventAction);
            parentPropertyObserver.NotifyingPropertyChanged += myDelegate;
            parentPropertyObserver.NotifyingPropertyNames.Add (propGetter.GetPropertyName ());
            myOtherObservers.Add (parentPropertyObserver);

            // these variables will be captured by the lambda
            var propGetterCompiled = propGetter.Compile ();
            var lastParentPropertyValue = (T1) null;
            var childPropertyObserver = (NotifyingPropertiesObserver) null;

            parentPropertyObserver.NotifyingPropertyChanged +=
                (sender, args) => {
                    var currentParentPropertyValue = propGetterCompiled ();
                    if (currentParentPropertyValue == null)
                    {
                        // no change in parent object
                        if (lastParentPropertyValue == null) return;

                        childPropertyObserver.ThrowIfNull ("subPropertyHandler");

                        // dispose of the chain against lastPropertyValue
                        childPropertyObserver.Dispose ();
                        childPropertyObserver = null;

                        lastParentPropertyValue = null;
                        return;
                    }

                    // no change in parent object
                    if (ReferenceEquals (lastParentPropertyValue, currentParentPropertyValue)) return;

                    // observer links ParentProperty.ChildProperty to notification chain
                    childPropertyObserver = new NotifyingPropertiesObserver (currentParentPropertyValue);
                    childPropertyObserver.NotifyingPropertyChanged += myDelegate;
                    childPropertyObserver.NotifyingPropertyNames.Add (subPropGetter.GetPropertyName ());

                    lastParentPropertyValue = currentParentPropertyValue;
                };

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