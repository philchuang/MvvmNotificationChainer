using System.ComponentModel;
using Com.PhilChuang.Utils.MvvmNotificationChainer;
using Demo.Utils;

// ReSharper disable InconsistentNaming
namespace MvvmNotificationChainer.UnitTests
{
    public class when_using_NotificationChainPropertyAttribute :
        when_testing_2deep_property_dependency_chain<
            when_using_NotificationChainPropertyAttribute_ViewModel,
            when_using_NotificationChainPropertyAttribute_LineItem>
    {
        protected override void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PropertyChangedOutput") return;
            base.OnPropertyChanged (sender, e);
        }
    }

    public class when_using_NotificationChainPropertyAttribute_ViewModel : NotifyPropertyChangedBase, when_testing_2deep_property_dependency_chain_IViewModel
    {
        private when_testing_2deep_property_dependency_chain_ILineItem myLineItem1;
        public when_testing_2deep_property_dependency_chain_ILineItem LineItem1
        {
            get { return myLineItem1; }
            set
            {
                myLineItem1 = value;
                RaisePropertyChanged ();
            }
        }

        private when_testing_2deep_property_dependency_chain_ILineItem myLineItem2;
        public when_testing_2deep_property_dependency_chain_ILineItem LineItem2
        {
            get { return myLineItem2; }
            set
            {
                myLineItem2 = value;
                RaisePropertyChanged ();
            }
        }

        private when_testing_2deep_property_dependency_chain_ILineItem myLineItem3;
        public when_testing_2deep_property_dependency_chain_ILineItem LineItem3
        {
            get { return myLineItem3; }
            set
            {
                myLineItem3 = value;
                RaisePropertyChanged ();
            }
        }

        [NotificationChainProperty]
        public int TotalLineItems
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                                          .Configure (cn => cn.On (() => LineItem1)
                                                              .On (() => LineItem2)
                                                              .On (() => LineItem3)
                                                              .Finish ());

                return (LineItem1 != null ? 1 : 0)
                       + (LineItem2 != null ? 1 : 0)
                       + (LineItem3 != null ? 1 : 0);
            }
        }

        [NotificationChainProperty]
        public int TotalItemQuantity
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                                          .Configure (cn => cn.On (() => LineItem1, li => li.Quantity)
                                                              .On (() => LineItem2, li => li.Quantity)
                                                              .On (() => LineItem3, li => li.Quantity)
                                                              .Finish ());

                return (LineItem1 != null ? LineItem1.Quantity : 0)
                       + (LineItem2 != null ? LineItem2.Quantity : 0)
                       + (LineItem3 != null ? LineItem3.Quantity : 0);
            }
        }

        [NotificationChainProperty]
        public decimal TotalCost
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                                          .Configure (cn => cn.On (() => LineItem1, li => li.Cost)
                                                              .On (() => LineItem2, li => li.Cost)
                                                              .On (() => LineItem3, li => li.Cost)
                                                              .Finish ());

                return (LineItem1 != null ? LineItem1.Cost : 0)
                       + (LineItem2 != null ? LineItem2.Cost : 0)
                       + (LineItem3 != null ? LineItem3.Cost : 0);
            }
        }
    }

    public class when_using_NotificationChainPropertyAttribute_LineItem : when_using_MvvmNotificationChainer_and_testing_simple_chain_ViewModel, when_testing_2deep_property_dependency_chain_ILineItem
    {
        [NotificationChainProperty]
        public override decimal Cost
        { get { return base.Cost; } }
    }
}