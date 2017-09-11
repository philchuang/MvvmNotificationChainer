using System.Collections.Generic;
using System.ComponentModel;

namespace MvvmNotificationChainer.UnitTests
{
    public abstract class NotificationTestBase : TestBase
    {
        protected INotifyPropertyChanged NotifyingObject { get; set; }
        protected List<string> ExpectedNotifications { get; set; } = new List<string>();
        protected List<string> ActualNotifications { get; set; } = new List<string>();

        protected void AssertNotificationsEqual()
        {
            AssertListEquals(ExpectedNotifications, ActualNotifications);
        }
    }
}
