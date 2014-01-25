using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Com.PhilChuang.Utils;
using Demo.Utils;
using JetBrains.Annotations;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace MvvmNotificationChainer.UnitTests
{
    public abstract class when_testing_2deep_property_dependency_chain<TViewModel, TLineItem> : when_using_INotifyPropertyChanged
        where TViewModel : when_testing_2deep_property_dependency_chain<TViewModel, TLineItem>.IViewModel
        where TLineItem : when_testing_2deep_property_dependency_chain<TViewModel, TLineItem>.ILineItem
    {
        public interface ILineItem : INotifyPropertyChanged
        {
            /// <summary>
            /// Source property, item quantity
            /// </summary>
            int Quantity { get; set; }

            /// <summary>
            /// Source property, individual item price
            /// </summary>
            decimal Price { get; set; }

            /// <summary>
            /// Derived property, item quantity * individual item price
            /// </summary>
            decimal Cost { get; }
        }

        public interface IViewModel : INotifyPropertyChanged
        {
            ILineItem LineItem1 { get; set; }
            ILineItem LineItem2 { get; set; }
            ILineItem LineItem3 { get; set; }

            int TotalLineItems { get; }
            int TotalItemQuantity { get; }
            decimal TotalCost { get; }
        }

        protected TViewModel myViewModel;

        protected override void Establish_context ()
        {
            myViewModel = Activator.CreateInstance<TViewModel> ();
            myViewModel.PropertyChanged += OnPropertyChanged;

            //myViewModel.LineItem1 = Activator.CreateInstance<TLineItem> ();
            myExpectedNotifications.Add ("LineItem1");
            myExpectedNotifications.Add ("TotalLineItems");
            myExpectedNotifications.Add ("TotalItemQuantity");
            myExpectedNotifications.Add ("TotalCost");
            //myViewModel.LineItem1.Quantity = 1;
            myExpectedNotifications.Add ("TotalItemQuantity");
            myExpectedNotifications.Add ("TotalCost");
            //myViewModel.LineItem1.Price = 99.99m;
            myExpectedNotifications.Add ("TotalCost");
            //myViewModel.LineItem2 = Activator.CreateInstance<TLineItem> ();
            myExpectedNotifications.Add ("LineItem2");
            myExpectedNotifications.Add ("TotalLineItems");
            myExpectedNotifications.Add ("TotalItemQuantity");
            myExpectedNotifications.Add ("TotalCost");
            //myViewModel.LineItem2.Quantity = 2;
            myExpectedNotifications.Add ("TotalItemQuantity");
            myExpectedNotifications.Add ("TotalCost");
            //myViewModel.LineItem2.Price = 50.00m;
            myExpectedNotifications.Add ("TotalCost");
            //myViewModel.LineItem3 = Activator.CreateInstance<TLineItem> ();
            myExpectedNotifications.Add ("LineItem3");
            myExpectedNotifications.Add ("TotalLineItems");
            myExpectedNotifications.Add ("TotalItemQuantity");
            myExpectedNotifications.Add ("TotalCost");
            //myViewModel.LineItem3 = null;
            myExpectedNotifications.Add ("LineItem3");
            myExpectedNotifications.Add ("TotalLineItems");
            myExpectedNotifications.Add ("TotalItemQuantity");
            myExpectedNotifications.Add ("TotalCost");
        }

        protected virtual void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
        { myActualNotifications.Add (e.PropertyName); }

        protected override void Because_of ()
        {
            try
            {
                myViewModel.LineItem1 = Activator.CreateInstance<TLineItem> ();
                myViewModel.LineItem1.Quantity = 1;
                myViewModel.LineItem1.Price = 99.99m;
                myViewModel.LineItem2 = Activator.CreateInstance<TLineItem> ();
                myViewModel.LineItem2.Quantity = 2;
                myViewModel.LineItem2.Price = 50.00m;
                myViewModel.LineItem3 = Activator.CreateInstance<TLineItem> ();
                myViewModel.LineItem3 = null;
            }
            catch (Exception ex)
            {
                m_BecauseOfException = ex;
            }
        }
    }

    public class when_not_using_MvvmNotificationChainer_and_testing_2deep_chain :
        when_testing_2deep_property_dependency_chain<
            when_not_using_MvvmNotificationChainer_and_testing_2deep_chain.ViewModel,
            when_not_using_MvvmNotificationChainer_and_testing_2deep_chain.LineItem>
    {
        public class LineItem : when_not_using_MvvmNotificationChainer_and_testing_simple_chain.ViewModel, ILineItem
        {
        }

        public class ViewModel : NotifyPropertyChangedBase, IViewModel
        {
            private ILineItem myLineItem1;
            public ILineItem LineItem1
            {
                get { return myLineItem1; }
                set
                {
                    if (myLineItem1 != null && !ReferenceEquals (myLineItem1, value))
                        LineItem1.PropertyChanged -= OnLineItemPropertyChanged;
                    myLineItem1 = value;
                    if (myLineItem1 != null)
                        LineItem1.PropertyChanged += OnLineItemPropertyChanged;
                    RaisePropertyChanged ();
                    RaisePropertyChanged (() => TotalLineItems);
                    RaisePropertyChanged (() => TotalItemQuantity);
                    RaisePropertyChanged (() => TotalCost);
                }
            }

            private ILineItem myLineItem2;
            public ILineItem LineItem2
            {
                get { return myLineItem2; }
                set
                {
                    if (myLineItem2 != null && !ReferenceEquals (myLineItem2, value))
                        LineItem2.PropertyChanged -= OnLineItemPropertyChanged;
                    myLineItem2 = value;
                    if (value != null)
                        LineItem2.PropertyChanged += OnLineItemPropertyChanged;
                    RaisePropertyChanged ();
                    RaisePropertyChanged (() => TotalLineItems);
                    RaisePropertyChanged (() => TotalItemQuantity);
                    RaisePropertyChanged (() => TotalCost);
                }
            }

            private ILineItem myLineItem3;
            public ILineItem LineItem3
            {
                get { return myLineItem3; }
                set
                {
                    if (myLineItem3 != null && !ReferenceEquals (myLineItem3, value))
                        LineItem3.PropertyChanged -= OnLineItemPropertyChanged;
                    myLineItem3 = value;
                    if (value != null)
                        LineItem3.PropertyChanged += OnLineItemPropertyChanged;
                    RaisePropertyChanged ();
                    RaisePropertyChanged (() => TotalLineItems);
                    RaisePropertyChanged (() => TotalItemQuantity);
                    RaisePropertyChanged (() => TotalCost);
                }
            }

            private void OnLineItemPropertyChanged (object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == "Quantity")
                {
                    RaisePropertyChanged (() => TotalItemQuantity);
                }
                else if (e.PropertyName == "Cost")
                {
                    RaisePropertyChanged (() => TotalCost);
                }
            }

            public int TotalLineItems
            {
                get
                {
                    return (LineItem1 != null ? 1 : 0)
                           + (LineItem2 != null ? 1 : 0)
                           + (LineItem3 != null ? 1 : 0);
                }
            }

            public int TotalItemQuantity
            {
                get
                {
                    return (LineItem1 != null ? LineItem1.Quantity : 0)
                           + (LineItem2 != null ? LineItem2.Quantity : 0)
                           + (LineItem3 != null ? LineItem3.Quantity : 0);
                }
            }

            public decimal TotalCost
            {
                get
                {
                    return (LineItem1 != null ? LineItem1.Cost : 0)
                           + (LineItem2 != null ? LineItem2.Cost : 0)
                           + (LineItem3 != null ? LineItem3.Cost : 0);
                }
            }
        }

        protected override void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PropertyChangedOutput") return;
            base.OnPropertyChanged (sender, e);
        }

    }

    public class when_using_MvvmNotificationChainer_and_testing_2deep_chain :
        when_testing_2deep_property_dependency_chain<
            when_using_MvvmNotificationChainer_and_testing_2deep_chain.ViewModel,
            when_using_MvvmNotificationChainer_and_testing_2deep_chain.LineItem>
    {
        public class LineItem : when_using_MvvmNotificationChainer_and_testing_simple_chain.ViewModel, ILineItem
        {
        }

        public class ViewModel : NotifyPropertyChangedBase, IViewModel
        {
            private ILineItem myLineItem1;
            public ILineItem LineItem1
            {
                get { return myLineItem1; }
                set
                {
                    myLineItem1 = value;
                    InitializeLineItem (myLineItem1);
                    RaisePropertyChanged ();
                }
            }

            private ILineItem myLineItem2;
            public ILineItem LineItem2
            {
                get { return myLineItem2; }
                set
                {
                    myLineItem2 = value;
                    InitializeLineItem (myLineItem2);
                    RaisePropertyChanged ();
                }
            }

            private ILineItem myLineItem3;
            public ILineItem LineItem3
            {
                get { return myLineItem3; }
                set
                {
                    myLineItem3 = value;
                    InitializeLineItem (myLineItem3);
                    RaisePropertyChanged ();
                }
            }

            private void InitializeLineItem (ILineItem li)
            {
                if (li == null) return;

                // In a traditional MVVM app, this won't be necessary because the databinding will call the necessary getters for us and initialize the chains
                var cost = li.Cost;
            }

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

        protected override void Because_of ()
        {
            try
            {
                // call dependent properties to initialize the chain
                var totalLineItems = myViewModel.TotalLineItems;
                var totalQuantity = myViewModel.TotalItemQuantity;
                var totalCost = myViewModel.TotalCost;
                base.Because_of ();
            }
            catch (Exception ex)
            {
                m_BecauseOfException = ex;
            }
        }

        protected override void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PropertyChangedOutput") return;
            base.OnPropertyChanged (sender, e);
        }
    }
}
