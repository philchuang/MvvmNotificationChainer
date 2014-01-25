using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Com.PhilChuang.Utils.MvvmNotificationChainer;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace MvvmNotificationChainer.UnitTests
{
    public abstract class when_using_NotificationChain : MvvmNotificationChainer_UnitTests_Base
    {
        protected NotificationChainManager myManager;
        protected NotificationChain myChain;

        protected virtual String DependentPropertyName { get { return "DependentPropertyName"; } }

        protected override void Establish_context ()
        {
            base.Establish_context ();

            m_IsBecauseOfExceptionExpected = false;
            
            myManager = new NotificationChainManager();
            myChain = new NotificationChain (myManager, DependentPropertyName);
        }
    }

    public class when_using_NotificationChain_Configure_and_Finish : when_using_NotificationChain
    {
        private bool m_PreFinishConfigureActionCalled = false;
        private bool m_PostFinishConfigureActionCalled = false;
        private NotificationChain m_PreFinishConfigureChain;

        private const String m_PreFinishObservingProperty = "ObservingProperty1";
        private const String m_PostFinishObservingProperty = "ObservingProperty2";

        protected override void Because_of ()
        {
            try
            {
                myChain.Configure (cn => {
                                       m_PreFinishConfigureActionCalled = true;
                                       m_PreFinishConfigureChain = cn;
                                       cn.On (m_PreFinishObservingProperty);
                                   });
                myChain.Finish();
                myChain.Configure (cn => {
                                       m_PostFinishConfigureActionCalled = true;
                                       cn.On (m_PostFinishObservingProperty);
                                   });
            }
            catch (Exception ex)
            {
                m_BecauseOfException = ex;
            }
        }

        [Test]
        public void then_prefinish_configure_lambda_should_execute ()
        {
            Assert.IsTrue (m_PreFinishConfigureActionCalled);
        }

        [Test]
        public void then_prefinish_configure_lambda_chain_parameter_should_be_the_same ()
        {
            Assert.AreSame (myChain, m_PreFinishConfigureChain);
        }

        [Test]
        public void then_prefinish_configure_lambda_should_configure ()
        {
            Assert.IsTrue (myChain.ObservedPropertyNames.Contains (m_PreFinishObservingProperty));
        }

        [Test]
        public void then_postfinish_configure_lambda_should_not_execute ()
        {
            Assert.IsFalse (m_PostFinishConfigureActionCalled);
        }

        [Test]
        public void then_postfinish_configure_lambda_should_not_configure ()
        {
            Assert.IsFalse (myChain.ObservedPropertyNames.Contains (m_PostFinishObservingProperty));
        }
    }

    public abstract class when_using_NotificationChain_On_and_Publish : when_using_NotificationChain
    {
        protected abstract String ObservingProperty { get; }
        protected const String NotifyingProperty = "NotifyingProperty";

        protected readonly Object mySender = new object();

        protected bool myChainCallbackCalled = false;
        protected List<Tuple<Object, String, String>> myChainNotifications = new List<Tuple<object, string, string>> ();

        protected override void Establish_context ()
        {
            base.Establish_context ();

            myChain.AndCall ((sender, notifyingProperty, dependentProperty) => {
                                 myChainCallbackCalled = true;
                                 myChainNotifications.Add (new Tuple<object, string, string> (sender, notifyingProperty, dependentProperty));
                             });
        }

        protected override void Because_of ()
        {
            base.Because_of ();

            try
            {
                myChain.On (ObservingProperty);
                myChain.Publish (mySender, new PropertyChangedEventArgs (NotifyingProperty));
            }
            catch (Exception ex)
            {
                m_BecauseOfException = ex;   
            }
        }

        [Test]
        public void then_observed_property_should_be_configured ()
        {
            Assert.IsTrue(myChain.ObservedPropertyNames.Contains (ObservingProperty));
        }
    }

    public class when_using_NotificationChain_On_and_publishing_observed_property : when_using_NotificationChain_On_and_Publish
    {
        protected override String ObservingProperty { get { return NotifyingProperty; } }

        [Test]
        public void then_callback_should_be_called ()
        {
            Assert.IsTrue (myChainCallbackCalled);
            Assert.AreEqual (1, myChainNotifications.Count);
            Assert.AreEqual (mySender, myChainNotifications[0].Item1);
            Assert.AreEqual (NotifyingProperty, myChainNotifications[0].Item2);
            Assert.AreEqual (DependentPropertyName, myChainNotifications[0].Item3);
        }
    }

    public class when_using_NotificationChain_On_and_publishing_unobserved_property : when_using_NotificationChain_On_and_Publish
    {
        protected override String ObservingProperty { get { return "NOT NotifyingProperty"; } }

        [Test]
        public void then_callback_should_not_be_called ()
        {
            Assert.IsFalse (myChainCallbackCalled);
            Assert.AreEqual (0, myChainNotifications.Count);
        }
    }
}
