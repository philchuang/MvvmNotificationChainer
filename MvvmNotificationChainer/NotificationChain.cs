using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace Com.PhilChuang.Utils.MvvmNotificationChainer
{
    /// <summary>
    /// Defines a NotificationChain. Observes multiple notifying properties on multiple objects and triggers NotifyingPropertyChanged for the dependent property.
    /// </summary>
    public class NotificationChain : IDisposable
    {
        /// <summary>
        /// Fires when an observed property is changed. Can listen to this event directly or call AndCall().
        /// First String parameter is the notifying property. Second String parameter is the dependent property.
        /// TODO include object sender parameter?
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
        public NotificationChain (String dependentPropertyName)
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

        public NotificationChain AndSetDefaultNotifyingObject (INotifyPropertyChanged notifyingObject)
        {
            return AndSetDefaultNotifyingObject (notifyingObject, h => notifyingObject.PropertyChanged += h, h => notifyingObject.PropertyChanged -= h);
        }

        public NotificationChain AndSetDefaultNotifyingObject (Object notifyingObject,
                                                                 Action<PropertyChangedEventHandler> addEventAction,
                                                                 Action<PropertyChangedEventHandler> removeEventAction)
        {
            DefaultNotifyingObject = notifyingObject;
            DefaultAddEventAction = addEventAction;
            DefaultRemoveEventAction = removeEventAction;
            return this;
        }

        /// <summary>
        /// Performs the registration/setup action on the current NotificationChain (if not yet finished).
        /// </summary>
        /// <param name="registrationAction"></param>
        /// <returns></returns>
        public NotificationChain Register (Action<NotificationChain> registrationAction)
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
        public NotificationChain On<T> (Expression<Func<T>> propGetter)
        {
            return On (DefaultNotifyingObject, DefaultAddEventAction, DefaultRemoveEventAction, propGetter.GetPropertyName ());
        }

        /// <summary>
        /// Specifies property name to observe on the default notifying object.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public NotificationChain On (String propertyName)
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
        public NotificationChain On<T> (INotifyPropertyChanged notifyingObject, Expression<Func<T>> propGetter)
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
        public NotificationChain On (INotifyPropertyChanged notifyingObject, string propertyName)
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
        public NotificationChain On<T> (
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
        public NotificationChain On (
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
        public NotificationChain On<T1, T2> (
            Expression<Func<T1>> propGetter,
            Expression<Func<T1, T2>> subPropGetter)
            where T1 : class, INotifyPropertyChanged
        {
            if (IsFinished) return this;

            return On (DefaultNotifyingObject, DefaultAddEventAction, DefaultRemoveEventAction, propGetter, subPropGetter);
        }

        /// <summary>
        /// Specifies a property of type INotifyPropertyChanged to observe on the default notifying object, and sub-property to observe
        /// </summary>
        /// <typeparam name="T1">The top-level Property to observe on notifyingObject, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T2">The Property to observe on T1</typeparam>
        /// <typeparam name="T3">The Property to observe on T2</typeparam>
        /// <param name="prop1Getter"></param>
        /// <param name="prop2Getter"></param>
        /// <param name="prop3Getter"></param>
        /// <returns></returns>
        public NotificationChain On<T1, T2, T3> (
            Expression<Func<T1>> prop1Getter,
            Expression<Func<T1, T2>> prop2Getter,
            Expression<Func<T2, T3>> prop3Getter)
            where T1 : class, INotifyPropertyChanged
            where T2 : class, INotifyPropertyChanged
        {
            if (IsFinished) return this;

            return On (DefaultNotifyingObject, DefaultAddEventAction, DefaultRemoveEventAction, prop1Getter, prop2Getter, prop3Getter);
        }

        /// <summary>
        /// Specifies a notifying object, property of type INotifyPropertyChanged, and sub-property to observe
        /// </summary>
        /// <typeparam name="T1">The top-level Property to observe on notifyingObject, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T2">The Property to observe on T1</typeparam>
        /// <param name="notifyingObject"></param>
        /// <param name="addEventAction"></param>
        /// <param name="removeEventAction"></param>
        /// <param name="prop1Getter"></param>
        /// <param name="prop2Getter"></param>
        /// <returns></returns>
        public NotificationChain On<T1, T2> (
            Object notifyingObject,
            Action<PropertyChangedEventHandler> addEventAction,
            Action<PropertyChangedEventHandler> removeEventAction,
            Expression<Func<T1>> prop1Getter,
            Expression<Func<T1, T2>> prop2Getter)
            where T1 : class, INotifyPropertyChanged
        {
            if (IsFinished) return this;

            /* How this works (2 deep)
             * 1) create observer on notifying object, looking for prop1
             * 2) when prop1 notifies
             * 2.1) if prop1 is non-null and has not been observed, then create observer on prop1 looking for prop2
             * 2.2) if prop1 is non-null and has already been observed, do nothing
             * 2.3) if prop1 is null and has not been observed, do nothing
             * 2.4) if prop1 is null and has already been observed, dispose the prop1-prop2 observer
             */

            // chain Parent.Property to this.DependentPropertyName
            // have to create a separate observer despite having same parent notifying object because each one will behave differently
            var prop1Observer = new NotifyingPropertiesObserver (addEventAction, removeEventAction);
            prop1Observer.NotifyingPropertyChanged += myDelegate;
            prop1Observer.NotifyingPropertyNames.Add (prop1Getter.GetPropertyName ());
            myOtherObservers.Add (prop1Observer);

            // these variables will be captured by the lambda
            var prop1GetterCompiled = prop1Getter.Compile ();
            var prop1LastValue = (T1) null;
            var prop1Prop2Observer = (NotifyingPropertiesObserver) null;

            prop1Observer.NotifyingPropertyChanged +=
                (sender, args) => {
                    var prop1CurrentValue = prop1GetterCompiled ();
                    if (prop1CurrentValue == null)
                    {
                        // no change in parent object
                        if (prop1LastValue == null) return;

                        prop1Prop2Observer.ThrowIfNull ("prop1prop2Observer");

                        // dispose of the chain against prop1LastValue
                        prop1Prop2Observer.Dispose ();
                        prop1Prop2Observer = null;

                        prop1LastValue = null;
                        return;
                    }

                    // no change in parent object
                    if (ReferenceEquals (prop1LastValue, prop1CurrentValue)) return;

                    // observer links prop1.prop2 to notification chain
                    prop1Prop2Observer = new NotifyingPropertiesObserver (prop1CurrentValue);
                    prop1Prop2Observer.NotifyingPropertyChanged += myDelegate;
                    prop1Prop2Observer.NotifyingPropertyNames.Add (prop2Getter.GetPropertyName ());

                    prop1LastValue = prop1CurrentValue;
                };

            return this;
        }
        
        /// <summary>
        /// Specifies a notifying object, property of type INotifyPropertyChanged, and sub-property to observe
        /// </summary>
        /// <typeparam name="T1">The top-level Property to observe on notifyingObject, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T2">The Property to observe on T1</typeparam>
        /// <typeparam name="T3">The Property to observe on T2</typeparam>
        /// <param name="notifyingObject"></param>
        /// <param name="addEventAction"></param>
        /// <param name="removeEventAction"></param>
        /// <param name="prop1Getter"></param>
        /// <param name="prop2Getter"></param>
        /// <param name="prop3Getter"></param>
        /// <returns></returns>
        public NotificationChain On<T1, T2, T3> (
            Object notifyingObject,
            Action<PropertyChangedEventHandler> addEventAction,
            Action<PropertyChangedEventHandler> removeEventAction,
            Expression<Func<T1>> prop1Getter,
            Expression<Func<T1, T2>> prop2Getter,
            Expression<Func<T2, T3>> prop3Getter)
            where T1 : class, INotifyPropertyChanged
            where T2 : class, INotifyPropertyChanged
        {
            if (IsFinished) return this;

            // NOTE this is just an "unrolled" copy & paste modification to On<T1, T2> ()

            /* How this works (3 deep)
             * 1) create observer on notifying object, looking for prop1
             * 2) when prop1 notifies, evaluate it
             * 2.1) if prop1 is non-null and has not been observed, then create observer on prop1 looking for prop2
             * 2.1.1) when prop2 notifies
             * 2.1.1.1) if prop2 is non-null and has not been observed, then create observer on prop2 looking for prop3
             * 2.1.1.2) if prop2 is non-null and has already been observed, do nothing
             * 2.1.1.3) if prop2 is null and has not been observed, do nothing
             * 2.1.1.4) if prop2 is null and has already been observed, dispose the prop2-prop3 observer
             * 2.2) if prop1 is non-null and has already been observed, do nothing
             * 2.3) if prop1 is null and has not been observed, do nothing
             * 2.4) if prop1 is null and has already been observed, dispose the prop2-prop3 observer, dispose the prop1-prop2 observer
             */

            // chain Parent.Property to this.DependentPropertyName
            // have to create a separate observer despite having same parent notifying object because each one will behave differently
            var prop1Observer = new NotifyingPropertiesObserver (addEventAction, removeEventAction);
            prop1Observer.NotifyingPropertyChanged += myDelegate;
            prop1Observer.NotifyingPropertyNames.Add (prop1Getter.GetPropertyName ());
            myOtherObservers.Add (prop1Observer);

            // these variables will be captured by the lambda
            var prop1GetterCompiled = prop1Getter.Compile ();
            var prop1LastValue = (T1) null;
            var prop1Prop2Observer = (NotifyingPropertiesObserver) null;

            var prop2GetterCompiled = prop2Getter.Compile ();
            var prop2LastValue = (T2) null;
            var prop2Prop3Observer = (NotifyingPropertiesObserver) null;

            prop1Observer.NotifyingPropertyChanged +=
                (sender, args) => {
                    var prop1CurrentValue = prop1GetterCompiled ();
                    if (prop1CurrentValue == null)
                    {
                        // no change in prop1
                        if (prop1LastValue == null) return;

                        // dispose of the chain against prop2LastValue
                        if (prop2Prop3Observer != null) prop2Prop3Observer.Dispose ();
                        prop2Prop3Observer = null;
                        prop2LastValue = null;

                        // dispose of the chain against prop1LastValue
                        prop1Prop2Observer.ThrowIfNull ("prop1prop2Observer");
                        prop1Prop2Observer.Dispose ();
                        prop1Prop2Observer = null;
                        prop1LastValue = null;
                        return;
                    }

                    // no change in prop1 object
                    if (ReferenceEquals (prop1LastValue, prop1CurrentValue)) return;

                    // observer links prop2.prop3 to notification chain
                    prop1Prop2Observer = new NotifyingPropertiesObserver (prop1CurrentValue);
                    prop1Prop2Observer.NotifyingPropertyChanged += myDelegate;
                    prop1Prop2Observer.NotifyingPropertyNames.Add (prop2Getter.GetPropertyName ());

                    prop1Prop2Observer.NotifyingPropertyChanged +=
                        (sender2, args2) =>
                        {
                            var prop2CurrentValue = prop2GetterCompiled (prop1CurrentValue);
                            if (prop2CurrentValue == null)
                            {
                                // no change in prop2
                                if (prop2LastValue == null) return;

                                // dispose of the chain against prop2LastValue
                                prop2Prop3Observer.ThrowIfNull ("prop2prop3Observer");
                                prop2Prop3Observer.Dispose ();
                                prop2Prop3Observer = null;
                                prop2LastValue = null;
                                return;
                            }

                            // no change in prop2 object
                            if (ReferenceEquals (prop2LastValue, prop2CurrentValue)) return;

                            // observer links prop2.prop3 to notification chain
                            prop2Prop3Observer = new NotifyingPropertiesObserver (prop2CurrentValue);
                            prop2Prop3Observer.NotifyingPropertyChanged += myDelegate;
                            prop2Prop3Observer.NotifyingPropertyNames.Add (prop3Getter.GetPropertyName ());

                            prop2LastValue = prop2CurrentValue;
                        };

                    prop1LastValue = prop1CurrentValue;
                };

            return this;
        }

        /// <summary>
        /// Specifies a notifying object, property of type INotifyPropertyChanged, and sub-property to observe
        /// </summary>
        /// <typeparam name="T1">The top-level Property to observe on notifyingObject, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T2">The Property to observe on T1</typeparam>
        /// <typeparam name="T3">The Property to observe on T2</typeparam>
        /// <typeparam name="T4">The Property to observe on T3</typeparam>
        /// <param name="notifyingObject"></param>
        /// <param name="addEventAction"></param>
        /// <param name="removeEventAction"></param>
        /// <param name="prop1Getter"></param>
        /// <param name="prop2Getter"></param>
        /// <param name="prop3Getter"></param>
        /// <param name="prop4Getter"></param>
        /// <returns></returns>
        public NotificationChain On<T1, T2, T3, T4> (
            Object notifyingObject,
            Action<PropertyChangedEventHandler> addEventAction,
            Action<PropertyChangedEventHandler> removeEventAction,
            Expression<Func<T1>> prop1Getter,
            Expression<Func<T1, T2>> prop2Getter,
            Expression<Func<T2, T3>> prop3Getter,
            Expression<Func<T3, T4>> prop4Getter)
            where T1 : class, INotifyPropertyChanged
            where T2 : class, INotifyPropertyChanged
            where T3 : class, INotifyPropertyChanged
        {
            if (IsFinished) return this;

            // NOTE this is just an "unrolled" copy & paste modification to On<T1, T2, T3> ()

            /* How this works (4 deep)
             * 1) create observer on notifying object, looking for prop1
             * 2) when prop1 notifies
             * 2.1) if prop1 is non-null and has not been observed, then create observer on prop1 looking for prop2
             * 2.1.1) when prop2 notifies
             * 2.1.1.1) if prop2 is non-null and has not been observed, then create observer on prop2 looking for prop3
             * 2.1.1.1.1) when prop3 notifies
             * 2.1.1.1.1.1) if prop3 is non-null and has not been observed, then create observer on prop3 looking for prop4
             * 2.1.1.1.1.2) if prop3 is non-null and has already been observed, do nothing
             * 2.1.1.1.1.3) if prop3 is null and has not been observed, do nothing
             * 2.1.1.1.1.4) if prop3 is null and has already been observed, dispose the prop3-prop4 observer
             * 2.1.1.2) if prop2 is non-null and has already been observed, do nothing
             * 2.1.1.3) if prop2 is null and has not been observed, do nothing
             * 2.1.1.4) if prop2 is null and has already been observed, dispose the prop2-prop3 observer
             * 2.2) if prop1 is non-null and has already been observed, do nothing
             * 2.3) if prop1 is null and has not been observed, do nothing
             * 2.4) if prop1 is null and has already been observed, dispose the prop2-prop3 observer, dispose the prop1-prop2 observer
             */

            // chain Parent.Property to this.DependentPropertyName
            // have to create a separate observer despite having same parent notifying object because each one will behave differently
            var prop1Observer = new NotifyingPropertiesObserver (addEventAction, removeEventAction);
            prop1Observer.NotifyingPropertyChanged += myDelegate;
            prop1Observer.NotifyingPropertyNames.Add (prop1Getter.GetPropertyName ());
            myOtherObservers.Add (prop1Observer);

            // these variables will be captured by the lambda
            var prop1GetterCompiled = prop1Getter.Compile ();
            var prop1LastValue = (T1) null;
            var prop1Prop2Observer = (NotifyingPropertiesObserver) null;

            var prop2GetterCompiled = prop2Getter.Compile ();
            var prop2LastValue = (T2) null;
            var prop2Prop3Observer = (NotifyingPropertiesObserver) null;

            var prop3GetterCompiled = prop3Getter.Compile ();
            var prop3LastValue = (T3) null;
            var prop3Prop4Observer = (NotifyingPropertiesObserver) null;

            prop1Observer.NotifyingPropertyChanged +=
                (sender, args) => {
                    var prop1CurrentValue = prop1GetterCompiled ();
                    if (prop1CurrentValue == null)
                    {
                        // no change in prop1
                        if (prop1LastValue == null) return;

                        // dispose of the chain against prop3LastValue
                        if (prop3Prop4Observer != null) prop3Prop4Observer.Dispose ();
                        prop3Prop4Observer = null;
                        prop3LastValue = null;

                        // dispose of the chain against prop2LastValue
                        if (prop2Prop3Observer != null) prop2Prop3Observer.Dispose ();
                        prop2Prop3Observer = null;
                        prop2LastValue = null;

                        // dispose of the chain against prop1LastValue
                        prop1Prop2Observer.ThrowIfNull ("prop1prop2Observer");
                        prop1Prop2Observer.Dispose ();
                        prop1Prop2Observer = null;
                        prop1LastValue = null;
                        return;
                    }

                    // no change in prop1 object
                    if (ReferenceEquals (prop1LastValue, prop1CurrentValue)) return;

                    // observer links prop1.prop2 to notification chain
                    prop1Prop2Observer = new NotifyingPropertiesObserver (prop1CurrentValue);
                    prop1Prop2Observer.NotifyingPropertyChanged += myDelegate;
                    prop1Prop2Observer.NotifyingPropertyNames.Add (prop2Getter.GetPropertyName ());

                    prop1Prop2Observer.NotifyingPropertyChanged +=
                        (sender2, args2) =>
                        {
                            var prop2CurrentValue = prop2GetterCompiled (prop1CurrentValue);
                            if (prop2CurrentValue == null)
                            {
                                // no change in prop2
                                if (prop2LastValue == null) return;

                                // dispose of the chain against prop3LastValue
                                if (prop3Prop4Observer != null) prop3Prop4Observer.Dispose ();
                                prop3Prop4Observer = null;
                                prop3LastValue = null;

                                // dispose of the chain against prop2LastValue
                                prop2Prop3Observer.ThrowIfNull ("prop2Prop3Observer");
                                prop2Prop3Observer.Dispose ();
                                prop2Prop3Observer = null;
                                prop2LastValue = null;
                                return;
                            }

                            // no change in prop2 object
                            if (ReferenceEquals (prop2LastValue, prop2CurrentValue)) return;

                            // observer links prop2.prop3 to notification chain
                            prop2Prop3Observer = new NotifyingPropertiesObserver (prop2CurrentValue);
                            prop2Prop3Observer.NotifyingPropertyChanged += myDelegate;
                            prop2Prop3Observer.NotifyingPropertyNames.Add (prop3Getter.GetPropertyName ());

                            prop2Prop3Observer.NotifyingPropertyChanged +=
                                (sender3, args3) =>
                                {
                                    var prop3CurrentValue = prop3GetterCompiled (prop2CurrentValue);
                                    if (prop3CurrentValue == null)
                                    {
                                        // no change in prop3
                                        if (prop3LastValue == null) return;

                                        // dispose of the chain against prop3LastValue
                                        prop3Prop4Observer.ThrowIfNull ("prop3Prop4Observer");
                                        prop3Prop4Observer.Dispose ();
                                        prop3Prop4Observer = null;
                                        prop3LastValue = null;
                                        return;
                                    }

                                    // no change in prop3 object
                                    if (ReferenceEquals (prop3LastValue, prop3CurrentValue)) return;

                                    // observer links prop3.prop4 to notification chain
                                    prop3Prop4Observer = new NotifyingPropertiesObserver (prop3CurrentValue);
                                    prop3Prop4Observer.NotifyingPropertyChanged += myDelegate;
                                    prop3Prop4Observer.NotifyingPropertyNames.Add (prop4Getter.GetPropertyName ());

                                    prop3LastValue = prop3CurrentValue;
                                };

                            prop2LastValue = prop2CurrentValue;
                        };

                    prop1LastValue = prop1CurrentValue;
                };

            return this;
        }

        /// <summary>
        /// Specifies an action to invoke when a notifying property is changed. Multiple actions can be invoked.
        /// </summary>
        /// <param name="onNotifyingPropertyChanged"></param>
        /// <returns></returns>
        public NotificationChain AndCall (Action onNotifyingPropertyChanged)
        {
            if (IsFinished) return this;

            onNotifyingPropertyChanged.ThrowIfNull ("onNotifyingPropertyChanged");

            AndCall ((notifyingProperty, dependentProperty) => onNotifyingPropertyChanged ());

            return this;
        }

        /// <summary>
        /// Specifies an action to invoke when a notifying property is changed. Multiple actions can be invoked.
        /// </summary>
        /// <param name="onNotifyingPropertyChanged">First String parameter is the notifying property. Second String parameter is the dependent property</param>
        /// <returns></returns>
        public NotificationChain AndCall (Action<String, String> onNotifyingPropertyChanged)
        {
            if (IsFinished) return this;

            onNotifyingPropertyChanged.ThrowIfNull ("onNotifyingPropertyChanged");

            NotifyingPropertyChanged += onNotifyingPropertyChanged;

            return this;
        }

        /// <summary>
        /// Removes all callbacks
        /// </summary>
        /// <returns></returns>
        public NotificationChain AndClearCalls ()
        {
            if (IsFinished) return this;

            foreach (var d in NotifyingPropertyChanged.GetInvocationList ().Cast<Action<String, String>> ())
                NotifyingPropertyChanged -= d;

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