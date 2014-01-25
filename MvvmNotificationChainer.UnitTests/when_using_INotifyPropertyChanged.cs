using System.Collections.Generic;
using System.ComponentModel;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace MvvmNotificationChainer.UnitTests
{
    public abstract class when_using_INotifyPropertyChanged : MvvmNotificationChainer_UnitTests_Base
    {
        protected INotifyPropertyChanged myNotifyingObject;

        protected List<string> myExpectedNotifications = new List<string> ();
        protected List<string> myActualNotifications = new List<string> ();

        [Test]
        public void then_notifications_should_match ()
        {
            AssertListEquals (myExpectedNotifications, myActualNotifications);
        }
    }
}