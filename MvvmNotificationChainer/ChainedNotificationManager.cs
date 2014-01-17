using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Com.PhilChuang.Utils.MvvmNotificationChainer
{
    /// <summary>
    /// Contains multiple ChainedNotifications, intended to be used 1 per instance.
    /// Prevents duplication of ChainedNotifications by dependent property name.
    /// When disposing, calls Dispose on all ChainedNotifications.
    /// </summary>
    public class ChainedNotificationManager : IDisposable
    {
        private readonly Dictionary<String, ChainedNotification> myChainedNotifications = new Dictionary<string, ChainedNotification> ();

        public virtual void Dispose ()
        {
            foreach (var cn in myChainedNotifications.Values)
            {
                cn.Dispose ();
            }
            myChainedNotifications.Clear ();
        }

        /// <summary>
        /// Creates a new ChainedNotification for the calling property, or returns an existing instance
        /// </summary>
        /// <param name="dependentPropertyName">Name of the property that depends on other properties</param>
        /// <returns></returns>
        public ChainedNotification Create ([CallerMemberName] string dependentPropertyName = null)
        {
            dependentPropertyName.ThrowIfNull ("dependentPropertyName");

            ChainedNotification cn;
            if (!myChainedNotifications.TryGetValue (dependentPropertyName, out cn))
                cn = myChainedNotifications[dependentPropertyName] = new ChainedNotification (dependentPropertyName);

            return cn;
        }

        /// <summary>
        /// Creates a new ChainedNotification for the calling property
        /// </summary>
        /// <param name="dependentPropertyName">Name of the property that depends on other properties</param>
        /// <returns></returns>
        public ChainedNotification Get ([CallerMemberName] string dependentPropertyName = null)
        {
            dependentPropertyName.ThrowIfNull ("dependentPropertyName");

            ChainedNotification cn;
            return myChainedNotifications.TryGetValue (dependentPropertyName, out cn) ? cn : null;
        }

        /// <summary>
        /// Clears a ChainedNotification for the calling property
        /// </summary>
        /// <param name="dependentPropertyName">Name of the property that depends on other properties</param>
        /// <returns></returns>
        public void Clear ([CallerMemberName] string dependentPropertyName = null)
        {
            dependentPropertyName.ThrowIfNull ("dependentPropertyName");

            var cn = Get (dependentPropertyName);
            if (cn == null) return;
            cn.Dispose ();
            myChainedNotifications.Remove (dependentPropertyName);
        }
    }
}