using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace Com.PhilChuang.Utils.MvvmNotificationChainer
{
    public delegate void NotificationChainCallback (Object sender, String notifyingProperty, String dependentProperty);

    /// <summary>
    /// Defines a NotificationChain. Observes multiple notifying properties and triggers callbacks for the dependent property.
    /// </summary>
    public class NotificationChain : IDisposable
    {
        /// <summary>
        /// The Manager that publishes to this chain.
        /// </summary>
        public INotificationChainManager ParentManager { get; set; }

        /// <summary>
        /// Name of the property that depends on other properties (e.g. Cost depends on Quantity and Price)
        /// </summary>
        public String DependentPropertyName { get; private set; }

        private List<String> myObservedPropertyNames = new List<String> ();

        /// <summary>
        /// The properties being observed by this chain
        /// </summary>
        public IList<String> ObservedPropertyNames
        {
            get { return myObservedPropertyNames.ToList (); }
        }

        /// <summary>
        /// Whether or not the notification has been fully defined (if false, then modifications are still allowed)
        /// </summary>
        public bool IsFinished { get; private set; }

        public bool IsDisposed { get; private set; }

        private List<NotificationChainCallback> myCallbacks = new List<NotificationChainCallback> ();

        /// <summary>
        /// </summary>
        /// <param name="parentManager"></param>
        /// <param name="dependentPropertyName">Name of the depending property</param>
        public NotificationChain (INotificationChainManager parentManager, String dependentPropertyName)
        {
            parentManager.ThrowIfNull ("parentManager");
            dependentPropertyName.ThrowIfNull ("dependentPropertyName");

            ParentManager = parentManager;
            DependentPropertyName = dependentPropertyName;
        }

        public void Dispose ()
        {
            if (IsDisposed) return;

            ParentManager = null;

            myObservedPropertyNames.Clear ();
            myObservedPropertyNames = null;

            myCallbacks.Clear ();
            myCallbacks = null;

            IsDisposed = true;
        }

        /// <summary>
        /// Performs the configuration action on the current NotificationChain (if not yet Finished).
        /// </summary>
        /// <param name="configAction"></param>
        /// <returns></returns>
        public NotificationChain Configure (Action<NotificationChain> configAction)
        {
            if (IsFinished || IsDisposed) return this;

            configAction.ThrowIfNull ("configAction");

            configAction (this);

            return this;
        }

        /// <summary>
        /// Specifies a property (T1) to observe on the current notifying object.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="propGetter"></param>
        /// <returns></returns>
        public NotificationChain On<T1> (Expression<Func<T1>> propGetter)
        {
            if (IsFinished || IsDisposed) return this;

            propGetter.ThrowIfNull ("propGetter");

            return On (propGetter.GetPropertyName ());
        }

        /// <summary>
        /// Specifies a property name to observe on the current notifying object
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

        /// <summary>
        /// Specifies a property (T1) to observe on the current notifying object, and its sub-properties (T2+) to observe
        /// </summary>
        /// <typeparam name="T1">The property (T1) to observe on T0, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T2">The property (T2) to observe on T1</typeparam>
        /// <param name="prop1Getter"></param>
        /// <param name="prop2Getter"></param>
        /// <returns></returns>
        public NotificationChain On<T1, T2> (
            Expression<Func<T1>> prop1Getter,
            Expression<Func<T1, T2>> prop2Getter)
            where T1 : class, INotifyPropertyChanged
        {
            if (IsFinished || IsDisposed) return this;

            DeepOn ((sender, notifyingProperty, dependentProperty) => FireCallbacks (sender, notifyingProperty, DependentPropertyName),
                    prop1Getter,
                    prop2Getter);

            return this;
        }

        /// <summary>
        /// Specifies a property (T1) to observe on the current notifying object, and its sub-properties (T2+) to observe
        /// </summary>
        /// <typeparam name="T1">The property (T1) to observe on T0, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T2">The property (T2) to observe on T1, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T3">The property (T3) to observe on T2</typeparam>
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

            DeepOn ((sender, notifyingProperty, dependentProperty) => FireCallbacks (sender, notifyingProperty, DependentPropertyName),
                    prop1Getter,
                    prop2Getter,
                    prop3Getter);

            return this;
        }

        /// <summary>
        /// Specifies a property (T1) to observe on the current notifying object, and its sub-properties (T2+) to observe
        /// </summary>
        /// <typeparam name="T1">The property (T1) to observe on T0, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T2">The property (T2) to observe on T1, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T3">The property (T3) to observe on T2, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T4">The property (T4) to observe on T3</typeparam>
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

            DeepOn ((sender, notifyingProperty, dependentProperty) => FireCallbacks (sender, notifyingProperty, DependentPropertyName),
                    prop1Getter,
                    prop2Getter,
                    prop3Getter,
                    prop4Getter);

            return this;
        }

        /// <summary>
        /// Specifies a property (T1) to observe on the current notifying object (T0)
        /// </summary>
        /// <typeparam name="T0"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="propGetter"></param>
        /// <returns></returns>
        private NotificationChain On<T0, T1> (Expression<Func<T0, T1>> propGetter)
        {
            if (IsFinished || IsDisposed) return this;

            propGetter.ThrowIfNull ("propGetter");

            return On (propGetter.GetPropertyName ());
        }

        /// <summary>
        /// Specifies a property (T1) to observe on the current notifying object, and its sub-properties (T2+) to observe
        /// </summary>
        /// <typeparam name="T1">The property (T1) to observe on T0, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T2">The property (T2) to observe on T1, implements INotifyPropertyChanged</typeparam>
        /// <param name="topLevelCallback"></param>
        /// <param name="prop1Getter"></param>
        /// <param name="prop2Getter"></param>
        /// <returns></returns>
        private NotificationChain DeepOn<T1, T2> (
            NotificationChainCallback topLevelCallback,
            Expression<Func<T1>> prop1Getter,
            Expression<Func<T1, T2>> prop2Getter)
            where T1 : class, INotifyPropertyChanged
        {
            if (IsFinished || IsDisposed) return this;

            topLevelCallback.ThrowIfNull ("topLevelCallback");
            prop1Getter.ThrowIfNull ("prop1Getter");
            prop2Getter.ThrowIfNull ("prop2Getter");

            On (prop1Getter);

            var mgr = ParentManager.CreateOrGetManager (prop1Getter);

            mgr.CreateOrGet ("../" + DependentPropertyName)
               .On (prop2Getter)
               .AndCall (topLevelCallback);

            return this;
        }

        /// <summary>
        /// Specifies a property (T1) to observe on the current notifying object (T0), and its sub-properties (T2+) to observe
        /// </summary>
        /// <typeparam name="T0">The top-level (T0) notifyingObject, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T1">The property (T1) to observe on T0, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T2">The property (T2) to observe on T1, implements INotifyPropertyChanged</typeparam>
        /// <param name="topLevelCallback"></param>
        /// <param name="prop1Getter"></param>
        /// <param name="prop2Getter"></param>
        /// <returns></returns>
        private NotificationChain DeepOn<T0, T1, T2> (
            NotificationChainCallback topLevelCallback,
            Expression<Func<T0, T1>> prop1Getter,
            Expression<Func<T1, T2>> prop2Getter)
            where T0 : class, INotifyPropertyChanged
            where T1 : class, INotifyPropertyChanged
        {
            if (IsFinished || IsDisposed) return this;

            topLevelCallback.ThrowIfNull ("topLevelCallback");
            prop1Getter.ThrowIfNull ("prop1Getter");
            prop2Getter.ThrowIfNull ("prop2Getter");

            On (prop1Getter);

            var mgr = ParentManager.CreateOrGetManager (prop1Getter);

            mgr.CreateOrGet ("../" + DependentPropertyName)
               .On (prop2Getter)
               .AndCall (topLevelCallback);

            return this;
        }

        /// <summary>
        /// Specifies a property (T1) to observe on the current notifying object, and its sub-properties (T2+) to observe
        /// </summary>
        /// <typeparam name="T1">The property (T1) to observe on T0, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T2">The property (T2) to observe on T1, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T3">The property (T3) to observe on T2</typeparam>
        /// <param name="topLevelCallback"></param>
        /// <param name="prop1Getter"></param>
        /// <param name="prop2Getter"></param>
        /// <param name="prop3Getter"></param>
        /// <returns></returns>
        private NotificationChain DeepOn<T1, T2, T3> (
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
            prop2Getter.ThrowIfNull ("prop2Getter");
            prop3Getter.ThrowIfNull ("prop3Getter");

            On (prop1Getter);

            var mgr = ParentManager.CreateOrGetManager (prop1Getter);

            mgr.CreateOrGet ("../" + DependentPropertyName)
               .DeepOn (topLevelCallback, prop2Getter, prop3Getter)
               .AndCall (topLevelCallback);

            return this;
        }

        /// <summary>
        /// Specifies a property (T1) to observe on the current notifying object (T0), and its sub-properties (T2+) to observe
        /// </summary>
        /// <typeparam name="T0">The top-level (T0) notifyingObject, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T1">The property (T1) to observe on T0, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T2">The property (T2) to observe on T1, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T3">The property (T3) to observe on T2</typeparam>
        /// <param name="topLevelCallback"></param>
        /// <param name="prop1Getter"></param>
        /// <param name="prop2Getter"></param>
        /// <param name="prop3Getter"></param>
        /// <returns></returns>
        private NotificationChain DeepOn<T0, T1, T2, T3> (
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
            prop2Getter.ThrowIfNull ("prop2Getter");
            prop3Getter.ThrowIfNull ("prop3Getter");

            On (prop1Getter);

            var mgr = ParentManager.CreateOrGetManager (prop1Getter);

            mgr.CreateOrGet ("../" + DependentPropertyName)
               .DeepOn (topLevelCallback, prop2Getter, prop3Getter)
               .AndCall (topLevelCallback);

            return this;
        }

        /// <summary>
        /// Specifies a property (T1) to observe on the current notifying object, and its sub-properties (T2+) to observe
        /// </summary>
        /// <typeparam name="T1">The property (T1) to observe on T0, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T2">The property (T2) to observe on T1, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T3">The property (T3) to observe on T2, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T4">The property (T4) to observe on T3</typeparam>
        /// <param name="topLevelCallback"></param>
        /// <param name="prop1Getter"></param>
        /// <param name="prop2Getter"></param>
        /// <param name="prop3Getter"></param>
        /// <param name="prop4Getter"></param>
        /// <returns></returns>
        private NotificationChain DeepOn<T1, T2, T3, T4> (
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
            prop1Getter.ThrowIfNull ("prop1Getter");
            prop2Getter.ThrowIfNull ("prop2Getter");
            prop3Getter.ThrowIfNull ("prop3Getter");
            prop4Getter.ThrowIfNull ("prop4Getter");

            On (prop1Getter);

            var mgr = ParentManager.CreateOrGetManager (prop1Getter);

            mgr.CreateOrGet ("../" + DependentPropertyName)
               .DeepOn (topLevelCallback, prop2Getter, prop3Getter, prop4Getter)
               .AndCall (topLevelCallback);

            return this;
        }

        /// <summary>
        /// Specifies a property (T1) to observe on the current notifying object, and its sub-properties (T2+) to observe
        /// </summary>
        /// <typeparam name="T0">The top-level (T0) notifyingObject, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T1">The property (T1) to observe on T0, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T2">The property (T2) to observe on T1, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T3">The property (T3) to observe on T2, implements INotifyPropertyChanged</typeparam>
        /// <typeparam name="T4">The property (T4) to observe on T3</typeparam>
        /// <param name="topLevelCallback"></param>
        /// <param name="prop1Getter"></param>
        /// <param name="prop2Getter"></param>
        /// <param name="prop3Getter"></param>
        /// <param name="prop4Getter"></param>
        /// <returns></returns>
        private NotificationChain DeepOn<T0, T1, T2, T3, T4> (
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
            prop1Getter.ThrowIfNull ("prop1Getter");
            prop2Getter.ThrowIfNull ("prop2Getter");
            prop3Getter.ThrowIfNull ("prop3Getter");
            prop4Getter.ThrowIfNull ("prop4Getter");

            On (prop1Getter);

            var mgr = ParentManager.CreateOrGetManager (prop1Getter);

            mgr.CreateOrGet ("../" + DependentPropertyName)
               .DeepOn (topLevelCallback, prop2Getter, prop3Getter, prop4Getter);

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

            myCallbacks.Clear ();

            return this;
        }

        /// <summary>
        /// Indicates that the chain has been fully defined and prevents further configuration.
        /// </summary>
        public void Finish ()
        {
            if (IsFinished) return;

            IsFinished = true;
        }

        /// <summary>
        /// Pushes PropertyChangedEventArgs input for processing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns>whether or not the callbacks were triggered</returns>
        public bool Publish (Object sender, PropertyChangedEventArgs args)
        {
            if (!myObservedPropertyNames.Contains (args.PropertyName)) return false;

            FireCallbacks (sender, args.PropertyName, DependentPropertyName);
            return true;
        }

        private void FireCallbacks (Object sender, String notifyingProperty, String dependentProperty)
        {
            foreach (var c in myCallbacks.ToList ())
                c (sender, notifyingProperty, dependentProperty);
        }
    }
}