using System;
using System.ComponentModel;
using Com.PhilChuang.Utils.MvvmNotificationChainer;
using Demo.Utils;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace MvvmNotificationChainer.UnitTests
{
    public abstract class when_using_NotificationChainManager : MvvmNotificationChainer_UnitTests_Base
    {
        protected NotificationChainManager myManager;

        protected override void Establish_context ()
        {
            base.Establish_context ();

            m_IsBecauseOfExceptionExpected = false;

            myManager = new NotificationChainManager ();
        }
    }

    public class when_using_NotificationChainManager_ExecuteAllChains : when_using_NotificationChainManager
    {
        protected Object m_SenderActual = null;
        protected String m_PropertyNameActual = null;

        protected bool m_Chain1_Callback1_WasCalled = false;
        protected bool m_Chain1_Callback2_WasCalled = false;
        protected Object m_Chain1_Callback2_Sender = null;
        protected String m_Chain1_Callback2_PropertyName = null;
        protected String m_Chain1_Callback2_DependentPropertyName = null;

        protected bool m_Chain2_Callback1_WasCalled = false;
        protected bool m_Chain2_Callback2_WasCalled = false;
        protected Object m_Chain2_Callback2_Sender = null;
        protected String m_Chain2_Callback2_PropertyName = null;
        protected String m_Chain2_Callback2_DependentPropertyName = null;

        protected class DeepTestClass : NotifyPropertyChangedBase
        {
            private String myString;
            public String String
            {
                get { return myString; }
                set
                {
                    myString = value;
                    RaisePropertyChanged ();
                }
            }
        }

        protected DeepTestClass myDeepTestClass;
        protected bool m_myDeepTestClassChain_Callback2_WasCalled = false;

        protected override void Establish_context ()
        {
            base.Establish_context ();

            myManager.CreateOrGet ("Chain1")
                     .AndCall (() => m_Chain1_Callback1_WasCalled = true)
                     .AndCall ((sender, property, dependentProperty) =>
                               {
                                   m_Chain1_Callback2_WasCalled = true;
                                   m_Chain1_Callback2_Sender = sender;
                                   m_Chain1_Callback2_PropertyName = property;
                                   m_Chain1_Callback2_DependentPropertyName = dependentProperty;
                               });

            myManager.CreateOrGet ("Chain2")
                     .AndCall (() => m_Chain2_Callback1_WasCalled = true)
                     .AndCall ((sender, property, dependentProperty) =>
                               {
                                   m_Chain2_Callback2_WasCalled = true;
                                   m_Chain2_Callback2_Sender = sender;
                                   m_Chain2_Callback2_PropertyName = property;
                                   m_Chain2_Callback2_DependentPropertyName = dependentProperty;
                               });

            myDeepTestClass = new DeepTestClass ();

            myManager.CreateOrGet (() => myDeepTestClass)
                     .On (() => myDeepTestClass, dtc => dtc.String)
                     .AndCall (() => m_Chain2_Callback1_WasCalled = true)
                     .AndCall ((sender, property, dependentProperty) =>
                               {
                                   m_myDeepTestClassChain_Callback2_WasCalled = true;
                               });

            m_SenderActual = this;
            m_PropertyNameActual = Guid.NewGuid ().ToString ();
        }

        protected override void Because_of ()
        {
            try
            {
                myManager.ExecuteAllChains (m_SenderActual, new PropertyChangedEventArgs (m_PropertyNameActual));
            }
            catch (Exception ex)
            {
                m_BecauseOfException = ex;
            }
        }

        [Test]
        public void then_callbacks_are_executed ()
        {
            Assert.IsTrue (m_Chain1_Callback1_WasCalled, "m_Chain1_Callback1_WasCalled");
            Assert.IsTrue (m_Chain1_Callback2_WasCalled, "m_Chain1_Callback2_WasCalled");
            Assert.AreEqual (m_SenderActual, m_Chain1_Callback2_Sender, "m_Chain1_Callback2_Sender");
            Assert.AreEqual (m_PropertyNameActual, m_Chain1_Callback2_PropertyName, "m_Chain1_Callback2_PropertyName");
            Assert.AreEqual ("Chain1", m_Chain1_Callback2_DependentPropertyName, "m_Chain1_Callback2_DependentPropertyName");
            Assert.IsTrue (m_Chain2_Callback1_WasCalled, "m_Chain2_Callback1_WasCalled");
            Assert.IsTrue (m_Chain2_Callback2_WasCalled, "m_Chain2_Callback2_WasCalled");
            Assert.AreEqual (m_SenderActual, m_Chain2_Callback2_Sender, "m_Chain2_Callback2_Sender");
            Assert.AreEqual (m_PropertyNameActual, m_Chain2_Callback2_PropertyName, "m_Chain2_Callback2_PropertyName");
            Assert.AreEqual ("Chain2", m_Chain2_Callback2_DependentPropertyName, "m_Chain2_Callback2_DependentPropertyName");
            Assert.IsTrue (m_myDeepTestClassChain_Callback2_WasCalled, "m_myDeepTestClassChain_Callback2_WasCalled");
        }
    }
}