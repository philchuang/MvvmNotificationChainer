using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Com.PhilChuang.Utils;
using Com.PhilChuang.Utils.MvvmNotificationChainer;
using Demo.Utils;
using Xunit;

// ReSharper disable InconsistentNaming
namespace MvvmNotificationChainer.UnitTests
{
    public interface ISimpleCollectionViewModel : INotifyPropertyChanged
    {
        ObservableCollection<String> StringCollection { get; set; }

        int StringCollectionCount { get; }
    }

    public class SimpleCollectionViewModel : NotifyPropertyChangedBase, ISimpleCollectionViewModel
    {
        private ObservableCollection<String> myStringCollection;
        public ObservableCollection<String> StringCollection
        {
            get { return myStringCollection; }
            set
            {
                myStringCollection = value;
                RaisePropertyChanged();
            }
        }

        [NotificationChainProperty]
        public int StringCollectionCount
        {
            get
            {
                myNotificationChainManager.CreateOrGet()
                                          .Configure(cn => cn.OnCollection(() => StringCollection).Finish());

                return myStringCollection != null ? myStringCollection.Count : 0;
            }
        }
    }

    public class SimpleCollectionTests : NotificationTestBase
    {
        [Theory]
        [MemberData(nameof(TestCases_for_SimpleCollectionViewModel_with_simple_collection_should_notify))]
        public void SimpleCollectionViewModel_with_simple_collection_should_notify(ISimpleCollectionViewModel viewModel)
        {
            // ARRANGE
            viewModel.PropertyChanged += (_, e) => ActualNotifications.Add(e.PropertyName);

            ExpectedNotifications.AddRange(new[]
                                           {
                                               //ViewModel.StringCollection = new ObservableCollection<String>();
                                               "StringCollection",
                                               "StringCollectionCount",
                                               //ViewModel.StringCollection.Add("Hello");
                                               "StringCollectionCount",
                                               //ViewModel.StringCollection.Add("World");
                                               "StringCollectionCount",
                                           });

            // ACT
            viewModel.StringCollection = new ObservableCollection<String>();
            viewModel.StringCollection.Add("Hello");
            viewModel.StringCollection.Add("World");

            // ASSERT
            AssertNotificationsEqual();
        }

        public static IEnumerable<object[]> TestCases_for_SimpleCollectionViewModel_with_simple_collection_should_notify =
            new[]
            {
                new[] {new SimpleCollectionViewModel()},
            };
    }

    public class TwoDeepCollectionTests : NotificationTestBase
    {
        [Theory]
        [MemberData(nameof(TestCases_for_TwoDeepCollectionViewModel_should_notify))]
        public void TwoDeepCollectionViewModel_should_notify(IDeepCollectionViewModel viewModel)
        {
            // ARRANGE
            viewModel.PropertyChanged += (_, e) => ActualNotifications.Add(e.PropertyName);

            ExpectedNotifications.AddRange(new[]
                                           {
                                               //viewModel.LineItems = new ObservableCollection<ILineItemViewModel>();
                                               "LineItems",
                                               "TotalLineItems",
                                               "TotalItemQuantity",
                                               "TotalCost",
                                               //viewModel.AddNewLineItem();
                                               "TotalLineItems",
                                               "TotalItemQuantity",
                                               "TotalCost",
                                               //viewModel.LineItems[0].Quantity = 1;
                                               "TotalItemQuantity",
                                               "TotalCost",
                                               //viewModel.LineItems[0].Price = 99.99m;
                                               "TotalCost",
                                               //viewModel.AddNewLineItem();
                                               "TotalLineItems",
                                               "TotalItemQuantity",
                                               "TotalCost",
                                               //viewModel.LineItems[1].Quantity = 2;
                                               "TotalItemQuantity",
                                               "TotalCost",
                                               //viewModel.LineItems[1].Price = 50.00m;
                                               "TotalCost",
                                               //viewModel.AddNewLineItem();
                                               "TotalLineItems",
                                               "TotalItemQuantity",
                                               "TotalCost",
                                               //viewModel.LineItems.RemoveAt(2);
                                               "TotalLineItems",
                                               "TotalItemQuantity",
                                               "TotalCost",
                                               //viewModel.LineItems = newLineItems;
                                               "LineItems",
                                               "TotalLineItems",
                                               "TotalItemQuantity",
                                               "TotalCost",
                                           });

            // ACT
            viewModel.LineItems = new ObservableCollection<ILineItemViewModel>();
            viewModel.AddNewLineItem();
            viewModel.LineItems[0].Quantity = 1;
            viewModel.LineItems[0].Price = 99.99m;
            viewModel.AddNewLineItem();
            viewModel.LineItems[1].Quantity = 2;
            viewModel.LineItems[1].Price = 50.00m;
            viewModel.AddNewLineItem();
            viewModel.LineItems.RemoveAt(2);
            var newLineItems = new ObservableCollection<ILineItemViewModel>();
            var newLineItem1 = (ILineItemViewModel) Activator.CreateInstance(viewModel.LineItems[0].GetType());
            newLineItem1.Quantity = 1;
            newLineItem1.Price = 99.99m;
            newLineItems.Add(newLineItem1);
            viewModel.LineItems = newLineItems;

            // ASSERT
            AssertNotificationsEqual();
        }

        public static readonly IEnumerable<object[]> TestCases_for_TwoDeepCollectionViewModel_should_notify = new[]
        {
            new object[] { new TwoDeepCollectionViewModelManual() },
            new object[] { new TwoDeepCollectionViewModelChained() },
        };
    }

    public interface IDeepCollectionViewModel : INotifyPropertyChanged
    {
        ObservableCollection<ILineItemViewModel> LineItems { get; set; }

        void AddNewLineItem();

        int TotalLineItems { get; }
        int TotalItemQuantity { get; }
        decimal TotalCost { get; }
    }

    public class TwoDeepCollectionViewModelManual : NotifyPropertyChangedBase, IDeepCollectionViewModel
    {
        private ObservableCollection<ILineItemViewModel> myLineItems;
        public ObservableCollection<ILineItemViewModel> LineItems
        {
            get { return myLineItems; }
            set
            {
                if (value == null && myLineItems != null)
                    myLineItems.CollectionChanged -= OnLineItems_CollectionChanged;
                myLineItems = value;
                if (myLineItems != null)
                    myLineItems.CollectionChanged += OnLineItems_CollectionChanged;
                RaisePropertyChanged ();
                RaisePropertyChanged (() => TotalLineItems);
                RaisePropertyChanged (() => TotalItemQuantity);
                RaisePropertyChanged (() => TotalCost);
            }
        }

        public void AddNewLineItem() => myLineItems.Add(new LineItemViewModelManual());

        private void OnLineItems_CollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                foreach (ILineItemViewModel li in e.OldItems)
                    li.PropertyChanged -= OnLineItemPropertyChanged;
            if (e.NewItems != null)
                foreach (ILineItemViewModel li in e.NewItems)
                    li.PropertyChanged += OnLineItemPropertyChanged;

            RaisePropertyChanged (() => TotalLineItems);
            RaisePropertyChanged (() => TotalItemQuantity);
            RaisePropertyChanged (() => TotalCost);
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
        { get { return myLineItems != null ? myLineItems.Count : 0; } }

        public int TotalItemQuantity
        { get { return myLineItems != null ? myLineItems.Select (li => li.Quantity).Sum () : 0; } }

        public decimal TotalCost
        { get { return myLineItems != null ? myLineItems.Select (li => li.Cost).Sum () : 0; } }
    }

    public class TwoDeepCollectionViewModelChained : NotifyPropertyChangedBase, IDeepCollectionViewModel
    {
        private ObservableCollection<ILineItemViewModel> myLineItems = new ObservableCollection<ILineItemViewModel>();
        public ObservableCollection<ILineItemViewModel> LineItems
        {
            get { return myLineItems; }
            set
            {
                myLineItems = value;
                RaisePropertyChanged ();
            }
        }

        public void AddNewLineItem() => myLineItems.Add(new LineItemViewModelChained());

        [NotificationChainProperty]
        public int TotalLineItems
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                                          .Configure (cn => cn.OnCollection (() => LineItems)
                                                              .Finish ());

                return myLineItems != null ? myLineItems.Count : 0;
            }
        }

        [NotificationChainProperty]
        public int TotalItemQuantity
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                                          .Configure (cn => cn.OnCollection (() => LineItems, li => li.Quantity)
                                                              .Finish ());

                return myLineItems != null ? myLineItems.Select (li => li.Quantity).Sum () : 0;
            }
        }

        [NotificationChainProperty]
        public decimal TotalCost
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                                          .Configure (cn => cn.OnCollection (() => LineItems, li => li.Cost)
                                                              .Finish ());

                return myLineItems != null ? myLineItems.Select (li => li.Cost).Sum () : 0;
            }
        }
    }

    public class ThreeDeepCollectionTests : NotificationTestBase
    {
        [Theory]
        [MemberData(nameof(TestCases_for_ThreeDeepCollectionViewModel_should_notify))]
        public void ThreeDeepCollectionViewModel_should_notify(IThreeDeepCollectionViewModel viewModel)
        {
            viewModel.PropertyChanged += (_, e) => ActualNotifications.Add(e.PropertyName);

            //viewModel.Orders = new ObservableCollection<IOrderViewModel>();
            ExpectedNotifications.Add("Orders");
            ExpectedNotifications.Add("TotalOrders");
            ExpectedNotifications.Add("TotalLineItems");
            ExpectedNotifications.Add("TotalItemQuantity");
            ExpectedNotifications.Add("TotalCost");
            //viewModel.AddNewOrder();
            ExpectedNotifications.Add("TotalOrders");
            ExpectedNotifications.Add("TotalLineItems");
            ExpectedNotifications.Add("TotalItemQuantity");
            ExpectedNotifications.Add("TotalCost");
            //viewModel.Orders[0].LineItems = new ObservableCollection<ILineItemViewModel>();
            ExpectedNotifications.Add("TotalLineItems");
            ExpectedNotifications.Add("TotalItemQuantity");
            ExpectedNotifications.Add("TotalCost");
            //viewModel.Orders[0].AddNewLineItem();
            ExpectedNotifications.Add("TotalLineItems");
            ExpectedNotifications.Add("TotalItemQuantity");
            ExpectedNotifications.Add("TotalCost");
            //viewModel.Orders[0].LineItems[0].Quantity = 1;
            ExpectedNotifications.Add("TotalItemQuantity");
            ExpectedNotifications.Add("TotalCost");
            //viewModel.Orders[0].LineItems[0].Price = 100;
            ExpectedNotifications.Add("TotalCost");
            //viewModel.Orders[0].LineItems.Add (li);
            ExpectedNotifications.Add("TotalLineItems");
            ExpectedNotifications.Add("TotalItemQuantity");
            ExpectedNotifications.Add("TotalCost");
            //viewModel.Orders.Add(order);
            ExpectedNotifications.Add("TotalOrders");
            ExpectedNotifications.Add("TotalLineItems");
            ExpectedNotifications.Add("TotalItemQuantity");
            ExpectedNotifications.Add("TotalCost");
            //viewModel.Orders[1].LineItems[0].Quantity = 3;
            ExpectedNotifications.Add("TotalItemQuantity");
            ExpectedNotifications.Add("TotalCost");

            viewModel.Orders = new ObservableCollection<IOrderViewModel>();
            // Orders
            // TotalOrders
            // TotalLineItems
            // TotalItemQuantity
            // TotalCost
            viewModel.AddNewOrder();
            // TotalOrders
            // TotalLineItems
            // TotalItemQuantity
            // TotalCost
            viewModel.Orders[0].LineItems = new ObservableCollection<ILineItemViewModel>();
            // TotalLineItems
            // TotalItemQuantity
            // TotalCost
            viewModel.Orders[0].AddNewLineItem();
            // TotalLineItems
            // TotalItemQuantity
            // TotalCost
            viewModel.Orders[0].LineItems[0].Quantity = 1;
            // TotalItemQuantity
            // TotalCost
            viewModel.Orders[0].LineItems[0].Price = 100;
            // TotalCost
            var li = (ILineItemViewModel) Activator.CreateInstance(viewModel.Orders[0].LineItems[0].GetType());
            li.Quantity = 2;
            li.Price = 50;
            viewModel.Orders[0].LineItems.Add(li);
            // TotalLineItems
            // TotalItemQuantity
            // TotalCost
            var order = (IOrderViewModel) Activator.CreateInstance(viewModel.Orders[0].GetType());
            order.AddNewLineItem();
            order.LineItems[0].Quantity = 2;
            order.LineItems[0].Price = 33.33m;
            viewModel.Orders.Add(order);
            // TotalOrders
            // TotalLineItems
            // TotalItemQuantity
            // TotalCost
            viewModel.Orders[1].LineItems[0].Quantity = 3;
            // TotalItemQuantity
            // TotalCost
        }

        public static readonly IEnumerable<object[]> TestCases_for_ThreeDeepCollectionViewModel_should_notify = new[]
        {
            new object[] { new ThreeDeepCollectionViewModelChained() },
        };
    }

    /// <summary>
    /// Order History
    /// </summary>
    public interface IThreeDeepCollectionViewModel : INotifyPropertyChanged
    {
        ObservableCollection<IOrderViewModel> Orders { get; set; }

        void AddNewOrder();

        int TotalOrders { get; }
        int TotalLineItems { get; }
        int TotalItemQuantity { get; }
        decimal TotalCost { get; }
    }

    public interface IOrderViewModel : INotifyPropertyChanged
    {
        ObservableCollection<ILineItemViewModel> LineItems { get; set; }

        void AddNewLineItem();

        int TotalLineItems { get; }
        int TotalItemQuantity { get; }
        decimal TotalCost { get; }
    }

    public class ThreeDeepCollectionViewModelChained : NotifyPropertyChangedBase, IThreeDeepCollectionViewModel
    {
        private ObservableCollection<IOrderViewModel> myOrders = new ObservableCollection<IOrderViewModel>();
        public ObservableCollection<IOrderViewModel> Orders
        {
            get { return myOrders; }
            set
            {
                myOrders = value;
                RaisePropertyChanged ();
            }
        }

        public void AddNewOrder() => myOrders.Add(new OrderViewModelChained());

        [NotificationChainProperty]
        public int TotalOrders
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                                          .Configure (cn => cn.OnCollection (() => Orders).Finish ());

                return Orders == null ? 0 : Orders.Count;
            }
        }

        [NotificationChainProperty]
        public int TotalLineItems
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                                          .Configure (cn => cn.OnCollection (() => Orders, o => o.TotalLineItems).Finish ());

                return Orders == null ? 0 : Orders.Select (o => o.LineItems == null ? 0 : o.LineItems.Count).Sum ();
            }
        }

        [NotificationChainProperty]
        public int TotalItemQuantity
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                                          .Configure (cn => cn.OnCollection (() => Orders, o => o.TotalItemQuantity).Finish ());

                return Orders == null ? 0 : Orders.Select (o => o.TotalItemQuantity).Sum ();
            }
        }

        [NotificationChainProperty]
        public decimal TotalCost
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                                          .Configure (cn => cn.OnCollection (() => Orders, o => o.TotalCost).Finish ());

                return Orders == null ? 0 : Orders.Select (o => o.TotalCost).Sum ();
            }

        }
    }

    public class OrderViewModelChained : NotifyPropertyChangedBase, IOrderViewModel
    {
        private ObservableCollection<ILineItemViewModel> myLineItems = new ObservableCollection<ILineItemViewModel>();
        public ObservableCollection<ILineItemViewModel> LineItems
        {
            get { return myLineItems; }
            set
            {
                myLineItems = value;
                RaisePropertyChanged ();
            }
        }

        public void AddNewLineItem() => myLineItems.Add(new LineItemViewModelChained());

        [NotificationChainProperty]
        public int TotalLineItems
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                                          .Configure (cn => cn.OnCollection (() => LineItems).Finish ());

                return LineItems == null ? 0 : LineItems.Count ();
            }
        }

        [NotificationChainProperty]
        public int TotalItemQuantity
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                                          .Configure (cn => cn.OnCollection (() => LineItems, o => o.Quantity).Finish ());

                return LineItems == null ? 0 : LineItems.Select (o => o.Quantity).Sum ();
            }
        }

        [NotificationChainProperty]
        public decimal TotalCost
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                                          .Configure (cn => cn.OnCollection (() => LineItems, o => o.Cost).Finish ());

                return LineItems == null ? 0 : LineItems.Select (o => o.Cost).Sum ();
            }
        }
    }
}
