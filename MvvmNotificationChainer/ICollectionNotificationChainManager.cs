using System.Collections.Generic;
using System.Collections.Specialized;

namespace Com.PhilChuang.Utils.MvvmNotificationChainer
{
    public interface ICollectionNotificationChainManager : INotificationChainManager
    {
        IEnumerable<INotifyCollectionChanged> ObservedCollections { get; }

        /// <summary>
        /// Begins observing the given notifying collection.
        /// </summary>
        /// <param name="notifyingCollection"></param>
        void ObserveCollection (INotifyCollectionChanged notifyingCollection);

        /// <summary>
        /// Stop observing the given notifying collection.
        /// </summary>
        /// <param name="notifyingCollection"></param>
        void StopObservingCollection (INotifyCollectionChanged notifyingCollection);
    }
}