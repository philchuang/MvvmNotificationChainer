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
    /// Contains multiple NotificationChains, intended to be used 1 per instance.
    /// Prevents duplication of NotificationChains by dependent property name.
    /// When disposing, calls Dispose on all NotificationChains.
    /// </summary>
    public class NotificationChainManager : IDisposable
    {
        private readonly Dictionary<String, NotificationChain> myChains = new Dictionary<string, NotificationChain> ();

        private object myDefaultNotifyingObject;
        private Action<PropertyChangedEventHandler> myDefaultAddEventAction;
        private Action<PropertyChangedEventHandler> myDefaultRemoveEventAction;
        private List<Action<String,String>> myDefaultCallbacks = new List<Action<string, string>> ();

        public virtual void Dispose ()
        {
            foreach (var chain in myChains.Values)
            {
                chain.Dispose ();
            }
            myChains.Clear ();

            myDefaultNotifyingObject = null;
            myDefaultAddEventAction = null;
            myDefaultRemoveEventAction = null;
            myDefaultCallbacks.Clear();
            myDefaultCallbacks = null;
        }

        public void SetDefaultNotifyingObject (INotifyPropertyChanged notifyingObject)
        {
            SetDefaultNotifyingObject (notifyingObject, h => notifyingObject.PropertyChanged += h, h => notifyingObject.PropertyChanged -= h);
        }

        public void SetDefaultNotifyingObject (
            Object notifyingObject,
            Action<PropertyChangedEventHandler> addEventAction,
            Action<PropertyChangedEventHandler> removeEventAction)
        {
            myDefaultNotifyingObject = notifyingObject;
            myDefaultAddEventAction = addEventAction;
            myDefaultRemoveEventAction = removeEventAction;
        }

        /// <summary>
        /// Specifies a default action to add to a new NotificationChain.
        /// </summary>
        /// <param name="onNotifyingPropertyChanged"></param>
        /// <returns></returns>
        public void AddDefaultCall (Action onNotifyingPropertyChanged)
        {
            onNotifyingPropertyChanged.ThrowIfNull ("onNotifyingPropertyChanged");

            AddDefaultCall ((notifyingProperty, dependentProperty) => onNotifyingPropertyChanged ());
        }

        /// <summary>
        /// Specifies an action to invoke when a notifying property is changed. Multiple actions can be invoked.
        /// </summary>
        /// <param name="onNotifyingPropertyChanged">First String parameter is the notifying property. Second String parameter is the dependent property</param>
        /// <returns></returns>
        public void AddDefaultCall (Action<String, String> onNotifyingPropertyChanged)
        {
            onNotifyingPropertyChanged.ThrowIfNull ("onNotifyingPropertyChanged");

            myDefaultCallbacks.Add (onNotifyingPropertyChanged);
        }

        /// <summary>
        /// Creates a new NotificationChain for the calling property, or returns an existing instance
        /// </summary>
        /// <param name="propGetter">Lambda expression for the property that depends on other properties</param>
        /// <returns></returns>
        public NotificationChain CreateOrGet<T> (Expression<Func<T>> propGetter)
        {
            propGetter.ThrowIfNull ("propGetter");

            return CreateOrGet (propGetter.GetPropertyName ());
        }

        /// <summary>
        /// Creates a new NotificationChain for the calling property, or returns an existing instance
        /// </summary>
        /// <param name="dependentPropertyName">Name of the property that depends on other properties</param>
        /// <returns></returns>
        public NotificationChain CreateOrGet ([CallerMemberName] string dependentPropertyName = null)
        {
            dependentPropertyName.ThrowIfNull ("dependentPropertyName");

            // TODO use manager to limit number of event handlers being added to notifying object
            NotificationChain chain;
            if (!myChains.TryGetValue (dependentPropertyName, out chain))
            {
                chain = myChains[dependentPropertyName] = new NotificationChain (dependentPropertyName);
                chain.AndSetDefaultNotifyingObject (myDefaultNotifyingObject, myDefaultAddEventAction, myDefaultRemoveEventAction);
                foreach (var callback in myDefaultCallbacks)
                    chain.AndCall (callback);
            }

            return chain;
        }

        /// <summary>
        /// Creates a new NotificationChain for the calling property
        /// </summary>
        /// <param name="dependentPropertyName">Name of the property that depends on other properties</param>
        /// <returns></returns>
        public NotificationChain Get ([CallerMemberName] string dependentPropertyName = null)
        {
            dependentPropertyName.ThrowIfNull ("dependentPropertyName");

            NotificationChain chain;
            return myChains.TryGetValue (dependentPropertyName, out chain) ? chain : null;
        }

        /// <summary>
        /// Clears a NotificationChain for the calling property
        /// </summary>
        /// <param name="dependentPropertyName">Name of the property that depends on other properties</param>
        /// <returns></returns>
        public void Clear ([CallerMemberName] string dependentPropertyName = null)
        {
            dependentPropertyName.ThrowIfNull ("dependentPropertyName");

            var chain = Get (dependentPropertyName);
            if (chain == null) return;
            chain.Dispose ();
            myChains.Remove (dependentPropertyName);
        }
    }
}