using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
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
            
            myManager = new NotificationChainManager ();
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

    public abstract class when_using_NotificationChain_OnRegex_and_Publish : when_using_NotificationChain
    {
        protected abstract String ObservingProperty { get; }
        protected const String NotifyingProperty = "NotifyingProperty";

        protected readonly Object mySender = new object ();

        protected bool myChainCallbackCalled = false;
        protected List<Tuple<Object, String, String>> myChainNotifications = new List<Tuple<object, string, string>> ();

        protected override void Establish_context ()
        {
            base.Establish_context ();

            myChain.AndCall ((sender, notifyingProperty, dependentProperty) =>
            {
                myChainCallbackCalled = true;
                myChainNotifications.Add (new Tuple<object, string, string> (sender, notifyingProperty, dependentProperty));
            });
        }

        protected override void Because_of ()
        {
            base.Because_of ();

            try
            {
                myChain.OnRegex (ObservingProperty);
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
            Assert.IsTrue (myChain.ObservedRegexes.Contains (ObservingProperty));
        }
    }

    public class when_using_NotificationChain_OnRegex_and_publishing_observed_property : when_using_NotificationChain_OnRegex_and_Publish
    {
        protected override String ObservingProperty { get { return "^Notifying.+$"; } }

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

    public class when_using_NotificationChain_OnRegex_and_publishing_unobserved_property : when_using_NotificationChain_OnRegex_and_Publish
    {
        protected override String ObservingProperty { get { return "NotifieingProperty"; } }

        [Test]
        public void then_callback_should_not_be_called ()
        {
            Assert.IsFalse (myChainCallbackCalled);
            Assert.AreEqual (0, myChainNotifications.Count);
        }
    }

    public class when_using_NotificationChain_On_with_Property : when_using_NotificationChain
    {
        public String Property { get; set; }

        protected Expression<Func<String>> m_Expression;
        protected String m_ExpectedPropertyName;

        protected override void Establish_context ()
        {
            base.Establish_context ();

            m_Expression = () => Property;
            m_ExpectedPropertyName = "Property";
        }

        protected override void Because_of ()
        {
            try
            {
                myChain.On (m_Expression);
            }
            catch (Exception ex)
            {
                m_BecauseOfException = ex;
            }
        }

        [Test]
        public void then_PropertyName_should_be_observed ()
        {
            Assert.IsTrue (myChain.ObservedPropertyNames.Contains (m_ExpectedPropertyName));
        }
    }

    public class when_using_NotificationChain_On_with_Field : when_using_NotificationChain
    {
        private String myProperty;

        protected Expression<Func<String>> m_Expression;
        protected String m_ExpectedPropertyName;

        protected override void Establish_context ()
        {
            base.Establish_context ();

            m_Expression = () => myProperty;
            m_ExpectedPropertyName = "myProperty";
        }

        protected override void Because_of ()
        {
            try
            {
                myChain.On (m_Expression);
            }
            catch (Exception ex)
            {
                m_BecauseOfException = ex;
            }
        }

        [Test]
        public void then_PropertyName_should_be_observed ()
        {
            Assert.IsTrue (myChain.ObservedPropertyNames.Contains (m_ExpectedPropertyName));
        }
    }

    public class when_using_NotificationChain_Execute : when_using_NotificationChain
    {
        protected Object m_SenderActual = null;
        protected String m_PropertyNameActual = null;

        protected Object m_Callback2_Sender = null;
        protected String m_Callback2_PropertyName = null;
        protected String m_Callback2_DependentPropertyName = null;

        protected bool m_Callback1_WasCalled = false;
        protected bool m_Callback2_WasCalled = false;

        protected override void Establish_context ()
        {
            base.Establish_context ();

            myChain.AndCall (() => m_Callback1_WasCalled = true);
            myChain.AndCall ((sender, property, dependentProperty) =>
                             {
                                 m_Callback2_WasCalled = true;
                                 m_Callback2_Sender = sender;
                                 m_Callback2_PropertyName = property;
                                 m_Callback2_DependentPropertyName = dependentProperty;
                             });

            m_SenderActual = this;
            m_PropertyNameActual = Guid.NewGuid ().ToString ();
        }

        protected override void Because_of ()
        {
            try
            {
                myChain.Execute (m_SenderActual, m_PropertyNameActual);
            }
            catch (Exception ex)
            {
                m_BecauseOfException = ex;
            }
        }

        [Test]
        public void then_callbacks_are_executed ()
        {
            Assert.IsTrue (m_Callback1_WasCalled, "m_Callback1_WasCalled");
            Assert.IsTrue (m_Callback2_WasCalled, "m_Callback2_WasCalled");
            Assert.AreEqual (m_SenderActual, m_Callback2_Sender, "m_Callback2_Sender");
            Assert.AreEqual (m_PropertyNameActual, m_Callback2_PropertyName, "m_Callback2_PropertyName");
            Assert.AreEqual (DependentPropertyName, m_Callback2_DependentPropertyName, "m_Callback2_DependentPropertyName");
        }
    }

    public class when_using_NotificationChain_Execute_no_params : when_using_NotificationChain
    {
        protected bool m_Callback1_WasCalled = false;
        protected bool m_Callback2_WasCalled = false;
        protected bool m_Callback3_WasCalled = false;

        protected override void Establish_context ()
        {
            base.Establish_context ();

            myChain.AndCall (() => m_Callback1_WasCalled = true);
            myChain.AndCall (() => m_Callback2_WasCalled = true);
        }

        protected override void Because_of ()
        {
            try
            {
                myChain.Execute ();
            }
            catch (Exception ex)
            {
                m_BecauseOfException = ex;
            }
        }

        [Test]
        public void then_callbacks_are_executed ()
        {
            Assert.IsTrue (m_Callback1_WasCalled, "m_Callback1_WasCalled");
            Assert.IsTrue (m_Callback2_WasCalled, "m_Callback2_WasCalled");
            Assert.IsFalse (m_Callback3_WasCalled, "m_Callback3_WasCalled");
        }
    }

    public class when_using_NotificationChain_FinishAndExecute : when_using_NotificationChain
    {
        protected Object m_Callback2_SenderActual = null;
        protected String m_Callback2_PropertyNameActual = null;

        protected Object m_Callback2_Sender = null;
        protected String m_Callback2_PropertyName = null;
        protected String m_Callback2_DependentPropertyName = null;

        protected bool m_Callback1_WasCalled = false;
        protected bool m_Callback2_WasCalled = false;

        protected override void Establish_context ()
        {
            base.Establish_context ();

            m_Callback2_SenderActual = this;
            m_Callback2_PropertyNameActual = Guid.NewGuid ().ToString ();
        }

        protected override void Because_of ()
        {
            try
            {
                myChain.AndCall (() => m_Callback1_WasCalled = true)
                       .AndCall ((sender, property, dependentProperty) =>
                                 {
                                     m_Callback2_WasCalled = true;
                                     m_Callback2_Sender = sender;
                                     m_Callback2_PropertyName = property;
                                     m_Callback2_DependentPropertyName = dependentProperty;
                                 })
                       .Finish (m_Callback2_SenderActual, m_Callback2_PropertyNameActual);
            }
            catch (Exception ex)
            {
                m_BecauseOfException = ex;
            }
        }

        [Test]
        public void then_callbacks_are_executed ()
        {
            Assert.IsTrue (m_Callback1_WasCalled, "m_Callback1_WasCalled");
            Assert.IsTrue (m_Callback2_WasCalled, "m_Callback2_WasCalled");
            Assert.AreEqual (m_Callback2_SenderActual, m_Callback2_Sender, "m_Callback2_Sender");
            Assert.AreEqual (m_Callback2_PropertyNameActual, m_Callback2_PropertyName, "m_Callback2_PropertyName");
            Assert.AreEqual (DependentPropertyName, m_Callback2_DependentPropertyName, "m_Callback2_DependentPropertyName");
        }
    }

    public class when_using_NotificationChain_Finish_false : when_using_NotificationChain
    {
        protected bool m_Callback1_WasCalled = false;
        protected bool m_Callback2_WasCalled = false;

        protected override void Because_of ()
        {
            try
            {
                myChain.AndCall (() => m_Callback1_WasCalled = true)
                       .AndCall (() => m_Callback2_WasCalled = true)
                       .Finish (false);
            }
            catch (Exception ex)
            {
                m_BecauseOfException = ex;
            }
        }

        [Test]
        public void then_callbacks_are_not_executed ()
        {
            Assert.IsFalse (m_Callback1_WasCalled, "m_Callback1_WasCalled");
            Assert.IsFalse (m_Callback2_WasCalled, "m_Callback2_WasCalled");
        }
    }
    public class when_using_NotificationChain_Finish_true : when_using_NotificationChain
    {
        protected bool m_Callback1_WasCalled = false;
        protected bool m_Callback2_WasCalled = false;

        protected override void Because_of ()
        {
            try
            {
                myChain.AndCall (() => m_Callback1_WasCalled = true)
                       .AndCall (() => m_Callback2_WasCalled = true)
                       .Finish (true);
            }
            catch (Exception ex)
            {
                m_BecauseOfException = ex;
            }
        }

        [Test]
        public void then_callbacks_are_executed ()
        {
            Assert.IsTrue (m_Callback1_WasCalled, "m_Callback1_WasCalled");
            Assert.IsTrue (m_Callback2_WasCalled, "m_Callback2_WasCalled");
        }
    }
}
