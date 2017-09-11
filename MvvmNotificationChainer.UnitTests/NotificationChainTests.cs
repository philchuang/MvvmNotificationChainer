using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Com.PhilChuang.Utils.MvvmNotificationChainer;
using Xunit;

// ReSharper disable InconsistentNaming
namespace MvvmNotificationChainer.UnitTests
{
    public abstract class NotificationChainTestBase : TestBase
    {
        protected NotificationChainManager Manager { get; }
        protected NotificationChain Chain { get; }

        protected string DependentPropertyName { get; }

        protected NotificationChainTestBase(string dependentPropertyName = "DependentPropertyName")
        {
            DependentPropertyName = dependentPropertyName;
            Manager = new NotificationChainManager();
            Chain = new NotificationChain(Manager, DependentPropertyName);
        }
    }

    public class NotificationChainConfigureTests: NotificationChainTestBase
    {
        [Fact]
        public void NotificationChain_should_Configure_until_Finish_called()
        {
            var preFinishObservingProperty = "ObservingProperty1";
            var postFinishObservingProperty = "ObservingProperty2";
            var preFinishConfigureActionCalled = false;
            var postFinishConfigureActionCalled = false;
            NotificationChain preFinishConfigureChain = null;

            Chain.Configure(cn =>
                            {
                                preFinishConfigureActionCalled = true;
                                preFinishConfigureChain = cn;
                                cn.On(preFinishObservingProperty);
                            });
            Chain.Finish();
            Chain.Configure(cn =>
                            {
                                postFinishConfigureActionCalled = true;
                                cn.On(postFinishObservingProperty);
                            });

            Assert.True(preFinishConfigureActionCalled);
            Assert.Same(Chain, preFinishConfigureChain);
            Assert.True(Chain.ObservedPropertyNames.Contains(preFinishObservingProperty));
            Assert.False(postFinishConfigureActionCalled);
            Assert.False(Chain.ObservedPropertyNames.Contains(postFinishObservingProperty));
        }
    }

    public class NotificationChainOnWithPropertyNameTests : NotificationChainTestBase
    {
        private string ObservingProperty;
        private const string NotifyingProperty = "NotifyingProperty";
        private bool ChainCallbackCalled = false;
        private readonly List<Tuple<object, string, string>> ChainNotifications = new List<Tuple<object, string, string>>();
        private readonly object Sender = new object();

        private void Act()
        {
            Chain.On(ObservingProperty);
            Chain.AndCall((sender, notifyingProperty, dependentProperty) =>
                          {
                              ChainCallbackCalled = true;
                              ChainNotifications.Add(new Tuple<object, string, string>(sender, notifyingProperty, dependentProperty));
                          });

            Chain.Publish(Sender, new PropertyChangedEventArgs(NotifyingProperty));
        }

        [Fact]
        public void NotificationChain_On_should_publish_for_observed_property()
        {
            ObservingProperty = NotifyingProperty;

            Act();

            Assert.True(Chain.ObservedPropertyNames.Contains(ObservingProperty));
            Assert.True(ChainCallbackCalled);
            Assert.Equal(1, ChainNotifications.Count);
            Assert.Equal(Sender, ChainNotifications[0].Item1);
            Assert.Equal(NotifyingProperty, ChainNotifications[0].Item2);
            Assert.Equal(DependentPropertyName, ChainNotifications[0].Item3);
        }

        [Fact]
        public void NotificationChain_On_should_not_publish_for_unobserved_property()
        {
            ObservingProperty = "NOT NotifyingProperty";

            Act();

            Assert.True(Chain.ObservedPropertyNames.Contains(ObservingProperty));
            Assert.False(ChainCallbackCalled);
            Assert.Equal(0, ChainNotifications.Count);
        }
    }

    public class NotificationChainOnRegexTests : NotificationChainTestBase
    {
        private string ObservingRegex;
        private const string NotifyingProperty = "NotifyingProperty";
        private bool ChainCallbackCalled = false;
        private readonly List<Tuple<object, string, string>> ChainNotifications = new List<Tuple<object, string, string>>();
        private readonly object Sender = new object();

        [Fact]
        public void NotificationChain_OnRegex_should_publish_for_observed_property()
        {
            ObservingRegex = "^Notifying.+$";

            Act();

            Assert.True(Chain.ObservedRegexes.Contains(ObservingRegex));
            Assert.True(ChainCallbackCalled);
            Assert.Equal(1, ChainNotifications.Count);
            Assert.Equal(Sender, ChainNotifications[0].Item1);
            Assert.Equal(NotifyingProperty, ChainNotifications[0].Item2);
            Assert.Equal(DependentPropertyName, ChainNotifications[0].Item3);
        }

        [Fact]
        public void NotificationChain_OnRegex_should_not_publish_for_unobserved_property()
        {
            ObservingRegex = "^NotNotifying.+$";

            Act();

            Assert.True(Chain.ObservedRegexes.Contains(ObservingRegex));
            Assert.False(ChainCallbackCalled);
            Assert.Equal(0, ChainNotifications.Count);
        }

        private void Act()
        {
            Chain.OnRegex(ObservingRegex);
            Chain.AndCall((sender, notifyingProperty, dependentProperty) =>
                          {
                              ChainCallbackCalled = true;
                              ChainNotifications.Add(new Tuple<object, string, string>(sender, notifyingProperty, dependentProperty));
                          });
            Chain.Publish(Sender, new PropertyChangedEventArgs(NotifyingProperty));
        }
    }

    public class NotificationChainOnWithExpressionTests : NotificationChainTestBase
    {
        public string Property { get; set; }
        private string myProperty;

        [Fact]
        public void NotificationChain_On_should_determine_property_name_from_expression()
        {
            Chain.On(() => Property);

            Assert.True(Chain.ObservedPropertyNames.Contains(nameof(Property)));
        }

        [Fact]
        public void NotificationChain_On_should_determine_field_name_from_expression()
        {
            Chain.On(() => myProperty);

            Assert.True(Chain.ObservedPropertyNames.Contains(nameof(myProperty)));
        }
    }

    public class NotificationChainExecuteTests : NotificationChainTestBase
    {
        [Fact]
        public void NotificationChain_Execute_should_execute_callbacks()
        {
            var senderExpected = this;
            var propertyNameExpected = Guid.NewGuid().ToString();

            object callback2_Sender = null;
            string callback2_PropertyName = null;
            string callback2_DependentPropertyName = null;

            var callback1_WasCalled = false;
            var callback2_WasCalled = false;

            Chain.AndCall(() => callback1_WasCalled = true);
            Chain.AndCall((sender, property, dependentProperty) =>
                          {
                              callback2_WasCalled = true;
                              callback2_Sender = sender;
                              callback2_PropertyName = property;
                              callback2_DependentPropertyName = dependentProperty;
                          });

            Chain.Execute(senderExpected, propertyNameExpected);

            Assert.True(callback1_WasCalled, nameof(callback1_WasCalled));
            Assert.True(callback2_WasCalled, nameof(callback2_WasCalled));
            Assert.Equal(senderExpected, callback2_Sender);
            Assert.Equal(propertyNameExpected, callback2_PropertyName);
            Assert.Equal(DependentPropertyName, callback2_DependentPropertyName);
        }

        [Fact]
        public void NotificationChain_Execute_with_no_params_should_execute_callbacks()
        {
            var callback1_WasCalled = false;
            var callback2_WasCalled = false;

            Chain.AndCall(() => callback1_WasCalled = true);
            Chain.AndCall(() => callback2_WasCalled = true);

            Chain.Execute();

            Assert.True(callback1_WasCalled, nameof(callback1_WasCalled));
            Assert.True(callback2_WasCalled, nameof(callback2_WasCalled));
        }

        [Fact]
        public void NotificationChain_Finish_with_params_should_execute_callbacks()
        {
            var callback2_SenderExpected = this;
            var callback2_PropertyNameActual = Guid.NewGuid().ToString();

            object callback2_Sender = null;
            string callback2_PropertyName = null;
            string callback2_DependentPropertyName = null;

            var callback1_WasCalled = false;
            var callback2_WasCalled = false;

            Chain.AndCall(() => callback1_WasCalled = true)
                 .AndCall((sender, property, dependentProperty) =>
                          {
                              callback2_WasCalled = true;
                              callback2_Sender = sender;
                              callback2_PropertyName = property;
                              callback2_DependentPropertyName = dependentProperty;
                          })
                 .Finish(callback2_SenderExpected, callback2_PropertyNameActual);

            Assert.True(callback1_WasCalled, nameof(callback1_WasCalled));
            Assert.True(callback2_WasCalled, nameof(callback2_WasCalled));
            Assert.Equal(callback2_SenderExpected, callback2_Sender);
            Assert.Equal(callback2_PropertyNameActual, callback2_PropertyName);
            Assert.Equal(DependentPropertyName, callback2_DependentPropertyName);
        }

        [Fact]
        public void NotificationChain_Finish_with_false_param_should_not_execute_callbacks()
        {
            var callback1_WasCalled = false;
            var callback2_WasCalled = false;

            Chain.AndCall(() => callback1_WasCalled = true)
                 .AndCall(() => callback2_WasCalled = true)
                 .Finish(false);

            Assert.False(callback1_WasCalled, nameof(callback1_WasCalled));
            Assert.False(callback2_WasCalled, nameof(callback2_WasCalled));
        }

        [Fact]
        public void NotificationChain_Finish_with_true_param_should_execute_callbacks()
        {
            var callback1_WasCalled = false;
            var callback2_WasCalled = false;

            Chain.AndCall(() => callback1_WasCalled = true)
                 .AndCall(() => callback2_WasCalled = true)
                 .Finish(true);

            Assert.True(callback1_WasCalled, nameof(callback1_WasCalled));
            Assert.True(callback2_WasCalled, nameof(callback2_WasCalled));
        }
    }
}
