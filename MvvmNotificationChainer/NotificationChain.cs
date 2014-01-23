using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace Com.PhilChuang.Utils.MvvmNotificationChainer
{
    public delegate void NotificationChainCallback (Object sender, String notifyingProperty, String dependentProperty);

    /// <summary>
    /// Defines a NotificationChain. Observes multiple notifying properties on multiple objects and triggers NotifyingPropertyChanged for the dependent property.
    /// </summary>
    public class NotificationChain : IDisposable
    {
        /// <summary>
        /// Name of the property that depends on other properties (e.g. Cost depends on Quantity and Price)
        /// </summary>
        public String DependentPropertyName { get; private set; }

        private List<String> myObservedPropertyNames = new List<string> ();
        public IList<String> ObservedPropertyNames
        { get { return myObservedPropertyNames.ToList (); } }

        /// <summary>
        /// Whether or not the notification has been fully defined (if false, then modifications are still allowed)
        /// </summary>
        public bool IsFinished { get; private set; }

        public bool IsDisposed { get; private set; }

        private List<NotificationChainCallback> myCallbacks = new List<NotificationChainCallback> (); 

        /// <summary>
        /// Map of dependent property name to notification chain manager
        /// </summary>
        private Dictionary<String, NotificationChainManager> myDeepChainManagers = new Dictionary<string, NotificationChainManager> ();

        /// <summary>
        /// Map of dependent property name to function to get that property value
        /// </summary>
        private Dictionary<String, Func<Object, Object>> myDeepChainGetters = new Dictionary<string, Func<Object, Object>> ();

        /// <summary>
        /// </summary>
        /// <param name="dependentPropertyName">Name of the depending property</param>
        public NotificationChain (String dependentPropertyName)
        {
            dependentPropertyName.ThrowIfNull ("dependentPropertyName");

            DependentPropertyName = dependentPropertyName;
        }

        public void Dispose ()
        {
            if (IsDisposed) return;

            myObservedPropertyNames.Clear ();
            myObservedPropertyNames = null;

            myCallbacks.Clear ();
            myCallbacks = null;

            foreach (var ncm in myDeepChainManagers.Values)
                ncm.Dispose();
            myDeepChainManagers.Clear ();
            myDeepChainManagers = null;

            myDeepChainGetters.Clear ();
            myDeepChainGetters = null;

            IsDisposed = true;
        }

        /// <summary>
        /// Performs the registration/setup action on the current NotificationChain (if not yet finished).
        /// </summary>
        /// <param name="registrationAction"></param>
        /// <returns></returns>
        public NotificationChain Register (Action<NotificationChain> registrationAction)
        {
            if (IsFinished || IsDisposed) return this;

            registrationAction.ThrowIfNull ("registrationAction");

            registrationAction (this);

            return this;
        }

        /// <summary>
        /// Specifies a property name to observe.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propGetter"></param>
        /// <returns></returns>
        public NotificationChain On<T> (Expression<Func<T>> propGetter)
        {
            if (IsFinished || IsDisposed) return this;

            propGetter.ThrowIfNull ("propGetter");

            return On (propGetter.GetPropertyName ());
        }

        /// <summary>
        /// Specifies a property name to observe.
        /// </summary>
        /// <typeparam name="T0"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="propGetter"></param>
        /// <returns></returns>
        public NotificationChain On<T0, T1> (Expression<Func<T0, T1>> propGetter)
        {
            if (IsFinished || IsDisposed) return this;

            propGetter.ThrowIfNull ("propGetter");

            return On (propGetter.GetPropertyName ());
        }

        /// <summary>
        /// Specifies a property name to observe.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public NotificationChain On (String propertyName)
        {
            if (IsFinished || IsDisposed) return this;

            propertyName.ThrowIfNullOrBlank ("propertyName");

            if (!myObservedPropertyNames.Contains (propertyName))
                myObservedPropertyNames.Add (propertyName);

            return this;
        }

        private NotificationChainManager CreateOrGetDeepManager<T1> (Expression<Func<T1>> propGetter)
        {
            var propName = propGetter.GetPropertyName ();

            NotificationChainManager mgr;
            if (!myDeepChainManagers.TryGetValue (propName, out mgr))
            {
                mgr = myDeepChainManagers[propName] = new NotificationChainManager ();
                myDeepChainGetters[propName] = _ => propGetter.Compile ().Invoke ();
            }
            
            return mgr;
        }

        private NotificationChainManager CreateOrGetDeepManager<T0, T1> (Expression<Func<T0, T1>> propGetter)
        {
            var propName = propGetter.GetPropertyName ();

            NotificationChainManager mgr;
            if (!myDeepChainManagers.TryGetValue (propName, out mgr))
            {
                mgr = myDeepChainManagers[propName] = new NotificationChainManager ();
                myDeepChainGetters[propName] = _ => propGetter.Compile ().Invoke (default (T0));
            }
            
            return mgr;
        }

        /// <summary>
        /// Specifies a property of type INotifyPropertyChanged to observe on the default notifying object, and sub-property to observe
        /// </summary>
        /// <typeparam name="T1">The Property (T1) to observe on T0, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T2">The Property (T2) to observe on T1, implements INotifyPropertyChanged</typeparam>
        /// <param name="prop1Getter"></param>
        /// <param name="prop2Getter"></param>
        /// <returns></returns>
        public NotificationChain On<T1, T2> (
            Expression<Func<T1>> prop1Getter,
            Expression<Func<T1, T2>> prop2Getter)
            where T1 : class, INotifyPropertyChanged
        {
            if (IsFinished || IsDisposed) return this;

            On ((sender, notifyingProperty, dependentProperty) => FireCallbacks (sender, notifyingProperty, DependentPropertyName),
                prop1Getter,
                prop2Getter);

            return this;
        }

        /// <summary>
        /// Specifies a property of type INotifyPropertyChanged to observe on the default notifying object, and sub-property to observe
        /// </summary>
        /// <typeparam name="T1">The Property (T1) to observe on T0, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T2">The Property (T2) to observe on T1, implements INotifyPropertyChanged</typeparam>
        /// <param name="topLevelCallback"></param>
        /// <param name="prop1Getter"></param>
        /// <param name="prop2Getter"></param>
        /// <returns></returns>
        private NotificationChain On<T1, T2> (
            NotificationChainCallback topLevelCallback,
            Expression<Func<T1>> prop1Getter,
            Expression<Func<T1, T2>> prop2Getter)
            where T1 : class, INotifyPropertyChanged
        {
            if (IsFinished || IsDisposed) return this;

            topLevelCallback.ThrowIfNull ("topLevelCallback");
            prop1Getter.ThrowIfNull ("prop1Getter");
            prop2Getter.ThrowIfNull ("prop2Getter");

            var mgr = CreateOrGetDeepManager (prop1Getter);

            mgr.CreateOrGet ("../" + DependentPropertyName)
               .On (prop2Getter)
               .AndCall (topLevelCallback);

            return this;
        }

        /// <summary>
        /// Specifies a property of type INotifyPropertyChanged to observe on the default notifying object, and sub-property to observe
        /// </summary>
        /// <typeparam name="T0">The top-level (T0) notifyingObject, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T1">The Property (T1) to observe on T0, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T2">The Property (T2) to observe on T1, implements INotifyPropertyChanged</typeparam>
        /// <param name="prop1Getter"></param>
        /// <param name="prop2Getter"></param>
        /// <returns></returns>
        public NotificationChain On<T0, T1, T2> (
            Expression<Func<T0, T1>> prop1Getter,
            Expression<Func<T1, T2>> prop2Getter)
            where T0 : class, INotifyPropertyChanged
            where T1 : class, INotifyPropertyChanged
        {
            if (IsFinished || IsDisposed) return this;

            On ((sender, notifyingProperty, dependentProperty) => FireCallbacks (sender, notifyingProperty, DependentPropertyName),
                prop1Getter,
                prop2Getter);

            return this;
        }

        /// <summary>
        /// Specifies a property of type INotifyPropertyChanged to observe on the default notifying object, and sub-property to observe
        /// </summary>
        /// <typeparam name="T0">The top-level (T0) notifyingObject, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T1">The Property (T1) to observe on T0, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T2">The Property (T2) to observe on T1, implements INotifyPropertyChanged</typeparam>
        /// <param name="topLevelCallback"></param>
        /// <param name="prop1Getter"></param>
        /// <param name="prop2Getter"></param>
        /// <returns></returns>
        private NotificationChain On<T0, T1, T2> (
            NotificationChainCallback topLevelCallback,
            Expression<Func<T0, T1>> prop1Getter,
            Expression<Func<T1, T2>> prop2Getter)
            where T0 : class, INotifyPropertyChanged
            where T1 : class, INotifyPropertyChanged
        {
            if (IsFinished || IsDisposed) return this;

            topLevelCallback.ThrowIfNull ("topLevelCallback");
            prop1Getter.ThrowIfNull ("prop1Getter");
            prop2Getter.ThrowIfNull("prop2Getter");

            var mgr = CreateOrGetDeepManager (prop1Getter);

            mgr.CreateOrGet ("../" + DependentPropertyName)
               .On (prop2Getter)
               .AndCall (topLevelCallback);

            return this;
        }

        /// <summary>
        /// Specifies a property of type INotifyPropertyChanged to observe on the default notifying object, and sub-property to observe
        /// </summary>
        /// <typeparam name="T1">The Property (T1) to observe on T0, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T2">The Property (T2) to observe on T1, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T3">The Property (T3) to observe on T2</typeparam>
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
            if (IsFinished || IsDisposed) return this;

            On ((sender, notifyingProperty, dependentProperty) => FireCallbacks (sender, notifyingProperty, DependentPropertyName),
                prop1Getter,
                prop2Getter,
                prop3Getter);

            return this;
        }

        /// <summary>
        /// Specifies a property of type INotifyPropertyChanged to observe on the default notifying object, and sub-property to observe
        /// </summary>
        /// <typeparam name="T1">The Property (T1) to observe on T0, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T2">The Property (T2) to observe on T1, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T3">The Property (T3) to observe on T2</typeparam>
        /// <param name="topLevelCallback"></param>
        /// <param name="prop1Getter"></param>
        /// <param name="prop2Getter"></param>
        /// <param name="prop3Getter"></param>
        /// <returns></returns>
        private NotificationChain On<T1, T2, T3> (
            NotificationChainCallback topLevelCallback,
            Expression<Func<T1>> prop1Getter,
            Expression<Func<T1, T2>> prop2Getter,
            Expression<Func<T2, T3>> prop3Getter)
            where T1 : class, INotifyPropertyChanged
            where T2 : class, INotifyPropertyChanged
        {
            if (IsFinished || IsDisposed) return this;

            topLevelCallback.ThrowIfNull ("topLevelCallback");
            prop1Getter.ThrowIfNull ("prop1Getter");
            prop2Getter.ThrowIfNull("prop2Getter");
            prop3Getter.ThrowIfNull("prop3Getter");

            var mgr = CreateOrGetDeepManager (prop1Getter);

            mgr.CreateOrGet ("../" + DependentPropertyName)
               .On (topLevelCallback, prop2Getter, prop3Getter);

            return this;
        }

        /// <summary>
        /// Specifies a property of type INotifyPropertyChanged to observe on the default notifying object, and sub-property to observe
        /// </summary>
        /// <typeparam name="T0">The top-level (T0) notifyingObject, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T1">The Property (T1) to observe on T0, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T2">The Property (T2) to observe on T1, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T3">The Property (T3) to observe on T2</typeparam>
        /// <param name="prop1Getter"></param>
        /// <param name="prop2Getter"></param>
        /// <param name="prop3Getter"></param>
        /// <returns></returns>
        public NotificationChain On<T0, T1, T2, T3> (
            Expression<Func<T0, T1>> prop1Getter,
            Expression<Func<T1, T2>> prop2Getter,
            Expression<Func<T2, T3>> prop3Getter)
            where T0 : class, INotifyPropertyChanged
            where T1 : class, INotifyPropertyChanged
            where T2 : class, INotifyPropertyChanged
        {
            if (IsFinished || IsDisposed) return this;

            On ((sender, notifyingProperty, dependentProperty) => FireCallbacks (sender, notifyingProperty, DependentPropertyName),
                prop1Getter,
                prop2Getter,
                prop3Getter);

            return this;
        }

        /// <summary>
        /// Specifies a property of type INotifyPropertyChanged to observe on the default notifying object, and sub-property to observe
        /// </summary>
        /// <typeparam name="T0">The top-level (T0) notifyingObject, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T1">The Property (T1) to observe on T0, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T2">The Property (T2) to observe on T1, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T3">The Property (T3) to observe on T2</typeparam>
        /// <param name="topLevelCallback"></param>
        /// <param name="prop1Getter"></param>
        /// <param name="prop2Getter"></param>
        /// <param name="prop3Getter"></param>
        /// <returns></returns>
        private NotificationChain On<T0, T1, T2, T3> (
            NotificationChainCallback topLevelCallback,
            Expression<Func<T0, T1>> prop1Getter,
            Expression<Func<T1, T2>> prop2Getter,
            Expression<Func<T2, T3>> prop3Getter)
            where T0 : class, INotifyPropertyChanged
            where T1 : class, INotifyPropertyChanged
            where T2 : class, INotifyPropertyChanged
        {
            if (IsFinished || IsDisposed) return this;

            topLevelCallback.ThrowIfNull ("topLevelCallback");
            prop1Getter.ThrowIfNull ("prop1Getter");
            prop2Getter.ThrowIfNull("prop2Getter");
            prop3Getter.ThrowIfNull("prop3Getter");

            var mgr = CreateOrGetDeepManager (prop1Getter);

            mgr.CreateOrGet ("../" + DependentPropertyName)
               .On (topLevelCallback, prop2Getter, prop3Getter);

            return this;
        }
        
        /// <summary>
        /// Specifies a property of type INotifyPropertyChanged to observe on the default notifying object, and sub-property to observe
        /// </summary>
        /// <typeparam name="T1">The Property (T1) to observe on T0, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T2">The Property (T2) to observe on T1, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T3">The Property (T3) to observe on T2, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T4">The Property (T4) to observe on T3</typeparam>
        /// <param name="prop1Getter"></param>
        /// <param name="prop2Getter"></param>
        /// <param name="prop3Getter"></param>
        /// <param name="prop4Getter"></param>
        /// <returns></returns>
        public NotificationChain On<T1, T2, T3, T4> (
            Expression<Func<T1>> prop1Getter,
            Expression<Func<T1, T2>> prop2Getter,
            Expression<Func<T2, T3>> prop3Getter,
            Expression<Func<T3, T4>> prop4Getter)
            where T1 : class, INotifyPropertyChanged
            where T2 : class, INotifyPropertyChanged
            where T3 : class, INotifyPropertyChanged
        {
            if (IsFinished || IsDisposed) return this;

            On ((sender, notifyingProperty, dependentProperty) => FireCallbacks (sender, notifyingProperty, DependentPropertyName),
                prop1Getter,
                prop2Getter,
                prop3Getter,
                prop4Getter);

            return this;
        }

        /// <summary>
        /// Specifies a property of type INotifyPropertyChanged to observe on the default notifying object, and sub-property to observe
        /// </summary>
        /// <typeparam name="T1">The Property (T1) to observe on T0, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T2">The Property (T2) to observe on T1, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T3">The Property (T3) to observe on T2, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T4">The Property (T4) to observe on T3</typeparam>
        /// <param name="topLevelCallback"></param>
        /// <param name="prop1Getter"></param>
        /// <param name="prop2Getter"></param>
        /// <param name="prop3Getter"></param>
        /// <param name="prop4Getter"></param>
        /// <returns></returns>
        private NotificationChain On<T1, T2, T3, T4> (
            NotificationChainCallback topLevelCallback,
            Expression<Func<T1>> prop1Getter,
            Expression<Func<T1, T2>> prop2Getter,
            Expression<Func<T2, T3>> prop3Getter,
            Expression<Func<T3, T4>> prop4Getter)
            where T1 : class, INotifyPropertyChanged
            where T2 : class, INotifyPropertyChanged
            where T3 : class, INotifyPropertyChanged
        {
            if (IsFinished || IsDisposed) return this;

            topLevelCallback.ThrowIfNull ("topLevelCallback");
            prop1Getter.ThrowIfNull("prop1Getter");
            prop2Getter.ThrowIfNull("prop2Getter");
            prop3Getter.ThrowIfNull("prop3Getter");
            prop4Getter.ThrowIfNull("prop4Getter");

            var mgr = CreateOrGetDeepManager (prop1Getter);

            mgr.CreateOrGet ("../" + DependentPropertyName)
               .On (topLevelCallback, prop2Getter, prop3Getter, prop4Getter);

            return this;
        }
        
        /// <summary>
        /// Specifies a property of type INotifyPropertyChanged to observe on the default notifying object, and sub-property to observe
        /// </summary>
        /// <typeparam name="T0">The top-level (T0) notifyingObject, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T1">The Property (T1) to observe on T0, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T2">The Property (T2) to observe on T1, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T3">The Property (T3) to observe on T2, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T4">The Property (T4) to observe on T3</typeparam>
        /// <param name="prop1Getter"></param>
        /// <param name="prop2Getter"></param>
        /// <param name="prop3Getter"></param>
        /// <param name="prop4Getter"></param>
        /// <returns></returns>
        public NotificationChain On<T0, T1, T2, T3, T4> (
            Expression<Func<T0, T1>> prop1Getter,
            Expression<Func<T1, T2>> prop2Getter,
            Expression<Func<T2, T3>> prop3Getter,
            Expression<Func<T3, T4>> prop4Getter)
            where T0 : class, INotifyPropertyChanged
            where T1 : class, INotifyPropertyChanged
            where T2 : class, INotifyPropertyChanged
            where T3 : class, INotifyPropertyChanged
        {
            if (IsFinished || IsDisposed) return this;

            On ((sender, notifyingProperty, dependentProperty) => FireCallbacks (sender, notifyingProperty, DependentPropertyName),
                prop1Getter,
                prop2Getter,
                prop3Getter,
                prop4Getter);

            return this;
        }

        /// <summary>
        /// Specifies a property of type INotifyPropertyChanged to observe on the default notifying object, and sub-property to observe
        /// </summary>
        /// <typeparam name="T0">The top-level (T0) notifyingObject, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T1">The Property (T1) to observe on T0, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T2">The Property (T2) to observe on T1, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T3">The Property (T3) to observe on T2, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T4">The Property (T4) to observe on T3</typeparam>
        /// <param name="topLevelCallback"></param>
        /// <param name="prop1Getter"></param>
        /// <param name="prop2Getter"></param>
        /// <param name="prop3Getter"></param>
        /// <param name="prop4Getter"></param>
        /// <returns></returns>
        private NotificationChain On<T0, T1, T2, T3, T4> (
            NotificationChainCallback topLevelCallback,
            Expression<Func<T0, T1>> prop1Getter,
            Expression<Func<T1, T2>> prop2Getter,
            Expression<Func<T2, T3>> prop3Getter,
            Expression<Func<T3, T4>> prop4Getter)
            where T0 : class, INotifyPropertyChanged
            where T1 : class, INotifyPropertyChanged
            where T2 : class, INotifyPropertyChanged
            where T3 : class, INotifyPropertyChanged
        {
            if (IsFinished || IsDisposed) return this;

            topLevelCallback.ThrowIfNull ("topLevelCallback");
            prop1Getter.ThrowIfNull("prop1Getter");
            prop2Getter.ThrowIfNull("prop2Getter");
            prop3Getter.ThrowIfNull("prop3Getter");
            prop4Getter.ThrowIfNull("prop4Getter");

            var mgr = CreateOrGetDeepManager (prop1Getter);

            mgr.CreateOrGet ("../" + DependentPropertyName)
               .On (topLevelCallback, prop2Getter, prop3Getter, prop4Getter);

            return this;
        }
        
        /// <summary>
        /// Specifies an action to invoke when a notifying property is changed. Multiple actions can be invoked.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public NotificationChain AndCall (Action callback)
        {
            if (IsFinished) return this;

            callback.ThrowIfNull ("callback");

            AndCall ((sender, notifyingProperty, dependentProperty) => callback ());

            return this;
        }

        /// <summary>
        /// Specifies an action to invoke when a notifying property is changed. Multiple actions can be invoked.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public NotificationChain AndCall (NotificationChainCallback callback)
        {
            if (IsFinished) return this;

            callback.ThrowIfNull ("callback");

            if (myCallbacks.Contains (callback)) return this;

            myCallbacks.Add (callback);

            return this;
        }

        /// <summary>
        /// Removes all callbacks
        /// </summary>
        /// <returns></returns>
        public NotificationChain AndClearCalls ()
        {
            if (IsFinished) return this;

            myCallbacks.Clear();

            return this;
        }

        private void FireCallbacks (Object sender, String notifyingProperty, String dependentProperty)
        {
            foreach (var c in myCallbacks.ToList ())
                c (sender, notifyingProperty, dependentProperty);
        }

        /// <summary>
        /// Indicates that the ChainedNotification has been fully defined and prevents further modification/registration.
        /// </summary>
        public void Finish ()
        {
            if (IsFinished) return;

            IsFinished = true;
        }

        public void Publish (Object sender, PropertyChangedEventArgs args)
        {
            if (myObservedPropertyNames.Contains (args.PropertyName))
                FireCallbacks (sender, args.PropertyName, DependentPropertyName);

            NotificationChainManager manager;
            if (myDeepChainManagers.TryGetValue (args.PropertyName, out manager))
            {
                var currentPropertyValue = (INotifyPropertyChanged) myDeepChainGetters[args.PropertyName] (sender);
                if (currentPropertyValue != null)
                {
                    if (ReferenceEquals (currentPropertyValue, manager.ObservedObject))
                        return; // no change

                    if (manager.ObservedObject != null)
                        manager.StopObserving ();
                    manager.Observe (currentPropertyValue);
                }
                else
                {
                    if (manager.ObservedObject == null)
                        return; // no change

                    manager.StopObserving();
                }
            }
        }
    }
}