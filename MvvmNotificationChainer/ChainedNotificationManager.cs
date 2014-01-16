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
    /// Prevents duplication of ChainedNotifications by chained property name.
    /// When disposing, calls Dispose on all ChainedNotifications.
    /// </summary>
    public class ChainedNotificationManager : IDisposable
    {
        private readonly Dictionary<String, ChainedNotification> myChainedNotifications = new Dictionary<string, ChainedNotification>();

        public virtual void Dispose()
        {
            foreach (var cnd in myChainedNotifications.Values)
            {
                cnd.Dispose();
            }
            myChainedNotifications.Clear();
        }

        /// <summary>
        /// Creates a new ChainedNotification for the calling property, or returns an existing instance
        /// </summary>
        /// <param name="chainedPropertyName">Name of the property that depends on other properties</param>
        /// <returns></returns>
        public ChainedNotification Create([CallerMemberName] string chainedPropertyName = null)
        {
            chainedPropertyName.ThrowIfNull("chainedPropertyName");

            ChainedNotification cnd;
            if (!myChainedNotifications.TryGetValue(chainedPropertyName, out cnd))
                cnd = myChainedNotifications[chainedPropertyName] = new ChainedNotification(chainedPropertyName);

            return cnd;
        }

        /// <summary>
        /// Creates a new ChainedNotification for the calling property
        /// </summary>
        /// <param name="chainedPropertyName">Name of the property that depends on other properties</param>
        /// <returns></returns>
        public ChainedNotification Get([CallerMemberName] string chainedPropertyName = null)
        {
            chainedPropertyName.ThrowIfNull("chainedPropertyName");

            ChainedNotification cnd;
            return myChainedNotifications.TryGetValue(chainedPropertyName, out cnd) ? cnd : null;
        }
    }
}
