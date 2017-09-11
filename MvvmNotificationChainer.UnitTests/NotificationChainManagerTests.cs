using System;
using System.ComponentModel;
using Com.PhilChuang.Utils.MvvmNotificationChainer;
using Demo.Utils;
using Xunit;

// ReSharper disable InconsistentNaming
namespace MvvmNotificationChainer.UnitTests
{
    public class NotificationChainManagerExecuteAllChainsTests : NotificationTestBase
    {
        protected class StringViewModel : NotifyPropertyChangedBase
        {
            private string myString;
            public string String
            {
                get { return myString; }
                set
                {
                    myString = value;
                    RaisePropertyChanged ();
                }
            }
        }

        [Fact]
        public void NotificationChainManager_ExecuteAllChains_should_execute_callbacks()
        {
            var senderExpected = this;
            var propertyNameExpected = Guid.NewGuid().ToString();

            var chain1_Callback1_WasCalled = false;
            var chain1_Callback2_WasCalled = false;
            object chain1_Callback2_Sender = null;
            string chain1_Callback2_PropertyName = null;
            string chain1_Callback2_DependentPropertyName = null;

            var chain2_Callback1_WasCalled = false;
            var chain2_Callback2_WasCalled = false;
            object chain2_Callback2_Sender = null;
            string chain2_Callback2_PropertyName = null;
            string chain2_Callback2_DependentPropertyName = null;
            var deepTestClassChain_Callback2_WasCalled = false;

            var manager = new NotificationChainManager();
            manager.CreateOrGet("Chain1")
                     .AndCall(() => chain1_Callback1_WasCalled = true)
                     .AndCall((sender, property, dependentProperty) =>
                              {
                                  chain1_Callback2_WasCalled = true;
                                  chain1_Callback2_Sender = sender;
                                  chain1_Callback2_PropertyName = property;
                                  chain1_Callback2_DependentPropertyName = dependentProperty;
                              });

            manager.CreateOrGet("Chain2")
                     .AndCall(() => chain2_Callback1_WasCalled = true)
                     .AndCall((sender, property, dependentProperty) =>
                              {
                                  chain2_Callback2_WasCalled = true;
                                  chain2_Callback2_Sender = sender;
                                  chain2_Callback2_PropertyName = property;
                                  chain2_Callback2_DependentPropertyName = dependentProperty;
                              });

            var testViewModel = new StringViewModel();

            manager.CreateOrGet(() => testViewModel)
                     .On(() => testViewModel, dtc => dtc.String)
                     .AndCall(() => chain2_Callback1_WasCalled = true)
                     .AndCall((sender, property, dependentProperty) =>
                              {
                                  deepTestClassChain_Callback2_WasCalled = true;
                              });

            manager.ExecuteAllChains(senderExpected, new PropertyChangedEventArgs(propertyNameExpected));

            Assert.True(chain1_Callback1_WasCalled, nameof(chain1_Callback1_WasCalled));
            Assert.True(chain1_Callback2_WasCalled, nameof(chain1_Callback2_WasCalled));
            Assert.Equal(senderExpected, chain1_Callback2_Sender);
            Assert.Equal(propertyNameExpected, chain1_Callback2_PropertyName);
            Assert.Equal("Chain1", chain1_Callback2_DependentPropertyName);

            Assert.True(chain2_Callback1_WasCalled, nameof(chain2_Callback1_WasCalled));
            Assert.True(chain2_Callback2_WasCalled, nameof(chain2_Callback2_WasCalled));
            Assert.Equal(senderExpected, chain2_Callback2_Sender);
            Assert.Equal(propertyNameExpected, chain2_Callback2_PropertyName);
            Assert.Equal("Chain2", chain2_Callback2_DependentPropertyName);
            Assert.True(deepTestClassChain_Callback2_WasCalled);
        }
    }
}