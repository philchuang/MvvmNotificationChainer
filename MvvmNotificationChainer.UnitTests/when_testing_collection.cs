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
using JetBrains.Annotations;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace MvvmNotificationChainer.UnitTests
{
    public class when_testing_collection_with_simple_type : when_using_INotifyPropertyChanged
    {
        public class ViewModel : NotifyPropertyChangedBase
        {
            private ObservableCollection<String> myStringCollection;
            public ObservableCollection<String> StringCollection
            {
                get { return myStringCollection; }
                set
                {
                    myStringCollection = value;
                    RaisePropertyChanged ();
                }
            }

            [NotificationChainProperty]
            public int StringCollectionCount
            {
                get
                {
                    myNotificationChainManager.CreateOrGet ()
                                              .Configure (cn => cn.OnCollection (() => StringCollection).Finish ());

                    return myStringCollection != null ? myStringCollection.Count : 0;
                }
            }
        }

        private ViewModel myViewModel;

        protected override void Establish_context ()
        {
            myViewModel = new ViewModel ();
            myViewModel.PropertyChanged += OnPropertyChanged;

            //myViewModel.StringCollection = new ObservableCollection<String>();
            myExpectedNotifications.Add ("StringCollection");
            myExpectedNotifications.Add ("StringCollectionCount");
            //myViewModel.StringCollection.Add("Hello");
            myExpectedNotifications.Add ("StringCollectionCount");
            //myViewModel.StringCollection.Add("World");
            myExpectedNotifications.Add ("StringCollectionCount");
        }

        protected virtual void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
        { myActualNotifications.Add (e.PropertyName); }

        protected override void Because_of ()
        {
            try
            {
                myViewModel.StringCollection = new ObservableCollection<String> ();
                myViewModel.StringCollection.Add ("Hello");
                myViewModel.StringCollection.Add ("World");
            }
            catch (Exception ex)
            {
                m_BecauseOfException = ex;
            }
        }
    }

    public abstract class when_testing_collection<TViewModel, TLineItem> : when_using_INotifyPropertyChanged
        where TViewModel : when_testing_collection_IViewModel
        where TLineItem : when_testing_2deep_property_dependency_chain_ILineItem
    {
        protected TViewModel myViewModel;

        protected override void Establish_context ()
        {
            myViewModel = Activator.CreateInstance<TViewModel> ();
            myViewModel.PropertyChanged += OnPropertyChanged;

            //myViewModel.LineItems = new ObservableCollection<when_testing_2deep_property_dependency_chain_ILineItem>();
            myExpectedNotifications.Add ("LineItems");
            myExpectedNotifications.Add ("TotalLineItems");
            myExpectedNotifications.Add ("TotalItemQuantity");
            myExpectedNotifications.Add ("TotalCost");
            //myViewModel.LineItems.Add (Activator.CreateInstance<TLineItem> ());
            myExpectedNotifications.Add ("TotalLineItems");
            myExpectedNotifications.Add ("TotalItemQuantity");
            myExpectedNotifications.Add ("TotalCost");
            //myViewModel.LineItems[0].Quantity = 1;
            myExpectedNotifications.Add ("TotalItemQuantity");
            myExpectedNotifications.Add ("TotalCost");
            //myViewModel.LineItems[0].Price = 99.99m;
            myExpectedNotifications.Add ("TotalCost");
            //myViewModel.LineItems.Add (Activator.CreateInstance<TLineItem> ());
            myExpectedNotifications.Add ("TotalLineItems");
            myExpectedNotifications.Add ("TotalItemQuantity");
            myExpectedNotifications.Add ("TotalCost");
            //myViewModel.LineItems[1].Quantity = 2;
            myExpectedNotifications.Add ("TotalItemQuantity");
            myExpectedNotifications.Add ("TotalCost");
            //myViewModel.LineItems[1].Price = 50.00m;
            myExpectedNotifications.Add ("TotalCost");
            //myViewModel.LineItems.Add (Activator.CreateInstance<TLineItem> ());
            myExpectedNotifications.Add ("TotalLineItems");
            myExpectedNotifications.Add ("TotalItemQuantity");
            myExpectedNotifications.Add ("TotalCost");
            //myViewModel.LineItems.RemoveAt (2);
            myExpectedNotifications.Add ("TotalLineItems");
            myExpectedNotifications.Add ("TotalItemQuantity");
            myExpectedNotifications.Add ("TotalCost");
            //myViewModel.LineItems = newLineItems;
            myExpectedNotifications.Add ("LineItems");
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
                myViewModel.LineItems = new ObservableCollection<when_testing_2deep_property_dependency_chain_ILineItem> ();
                myViewModel.LineItems.Add (Activator.CreateInstance<TLineItem> ());
                myViewModel.LineItems[0].Quantity = 1;
                myViewModel.LineItems[0].Price = 99.99m;
                myViewModel.LineItems.Add (Activator.CreateInstance<TLineItem> ());
                myViewModel.LineItems[1].Quantity = 2;
                myViewModel.LineItems[1].Price = 50.00m;
                myViewModel.LineItems.Add (Activator.CreateInstance<TLineItem> ());
                myViewModel.LineItems.RemoveAt (2);
                var newLineItems = new ObservableCollection<when_testing_2deep_property_dependency_chain_ILineItem> ();
                var newLineItem1 = Activator.CreateInstance<TLineItem> ();
                newLineItem1.Quantity = 1;
                newLineItem1.Price = 99.99m;
                newLineItems.Add (newLineItem1);
                myViewModel.LineItems = newLineItems;
            }
            catch (Exception ex)
            {
                m_BecauseOfException = ex;
            }
        }
    }

    public interface when_testing_collection_IViewModel : INotifyPropertyChanged
    {
        ObservableCollection<when_testing_2deep_property_dependency_chain_ILineItem> LineItems { get; set; }

        int TotalLineItems { get; }
        int TotalItemQuantity { get; }
        decimal TotalCost { get; }
    }

    public class when_not_using_MvvmNotificationChainer_and_testing_collection :
        when_testing_collection<
            when_not_using_MvvmNotificationChainer_and_testing_collection_ViewModel,
            when_not_using_MvvmNotificationChainer_and_testing_collection_LineItem>
    {
        protected override void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PropertyChangedOutput") return;
            base.OnPropertyChanged (sender, e);
        }
    }

    public class when_not_using_MvvmNotificationChainer_and_testing_collection_ViewModel : NotifyPropertyChangedBase, when_testing_collection_IViewModel
    {
        private ObservableCollection<when_testing_2deep_property_dependency_chain_ILineItem> myLineItems;
        public ObservableCollection<when_testing_2deep_property_dependency_chain_ILineItem> LineItems
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

        private void OnLineItems_CollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                foreach (when_testing_2deep_property_dependency_chain_ILineItem li in e.OldItems)
                    li.PropertyChanged -= OnLineItemPropertyChanged;
            if (e.NewItems != null)
                foreach (when_testing_2deep_property_dependency_chain_ILineItem li in e.NewItems)
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

    public class when_not_using_MvvmNotificationChainer_and_testing_collection_LineItem : when_not_using_MvvmNotificationChainer_and_testing_simple_chain_ViewModel, when_testing_2deep_property_dependency_chain_ILineItem
    {
    }

    public class when_using_MvvmNotificationChainer_and_collection :
        when_testing_collection<
            when_using_MvvmNotificationChainer_and_collection_ViewModel,
            when_using_MvvmNotificationChainer_and_collection_LineItem>
    {
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

    public class when_using_MvvmNotificationChainer_and_collection_ViewModel : NotifyPropertyChangedBase, when_testing_collection_IViewModel
    {
        private ObservableCollection<when_testing_2deep_property_dependency_chain_ILineItem> myLineItems;
        public ObservableCollection<when_testing_2deep_property_dependency_chain_ILineItem> LineItems
        {
            get { return myLineItems; }
            set
            {
                myLineItems = value;
                RaisePropertyChanged ();
            }
        }

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

    public class when_using_MvvmNotificationChainer_and_collection_LineItem : when_using_NotificationChainPropertyAttribute_LineItem
    {
    }

    public abstract class when_testing_collection_deep<TViewModel, TOrder, TLineItem> : when_using_INotifyPropertyChanged
        where TViewModel : when_testing_collection_deep_IViewModel
        where TOrder : when_testing_collection_deep_IOrder
        where TLineItem : when_testing_2deep_property_dependency_chain_ILineItem
    {
        protected TViewModel myViewModel;

        protected override void Establish_context ()
        {
            myViewModel = Activator.CreateInstance<TViewModel> ();
            myViewModel.PropertyChanged += OnPropertyChanged;

            //myViewModel.Orders = new ObservableCollection<when_testing_collection_deep_IOrder> ();
            myExpectedNotifications.Add ("Orders");
            myExpectedNotifications.Add ("TotalOrders");
            myExpectedNotifications.Add ("TotalLineItems");
            myExpectedNotifications.Add ("TotalItemQuantity");
            myExpectedNotifications.Add ("TotalCost");
            //myViewModel.Orders.Add (Activator.CreateInstance<TOrder> ());
            myExpectedNotifications.Add ("TotalOrders");
            myExpectedNotifications.Add ("TotalLineItems");
            myExpectedNotifications.Add ("TotalItemQuantity");
            myExpectedNotifications.Add ("TotalCost");
            //myViewModel.Orders[0].LineItems = new ObservableCollection<when_testing_2deep_property_dependency_chain_ILineItem> ();
            myExpectedNotifications.Add ("TotalLineItems");
            myExpectedNotifications.Add ("TotalItemQuantity");
            myExpectedNotifications.Add ("TotalCost");
            //myViewModel.Orders[0].LineItems.Add (Activator.CreateInstance<TLineItem> ());
            myExpectedNotifications.Add ("TotalLineItems");
            myExpectedNotifications.Add ("TotalItemQuantity");
            myExpectedNotifications.Add ("TotalCost");
            //myViewModel.Orders[0].LineItems[0].Quantity = 1;
            myExpectedNotifications.Add ("TotalItemQuantity");
            myExpectedNotifications.Add ("TotalCost");
            //myViewModel.Orders[0].LineItems[0].Price = 100;
            myExpectedNotifications.Add ("TotalCost");
            //var li = Activator.CreateInstance<TLineItem> ();
            //li.Quantity = 2;
            //li.Price = 50;
            //myViewModel.Orders[0].LineItems.Add (li);
            myExpectedNotifications.Add ("TotalLineItems");
            myExpectedNotifications.Add ("TotalItemQuantity");
            myExpectedNotifications.Add ("TotalCost");
            //var order = Activator.CreateInstance<TOrder> ();
            //li = Activator.CreateInstance<TLineItem> ();
            //li.Quantity = 2;
            //li.Price = 33.33m;
            //order.LineItems = new ObservableCollection<when_testing_2deep_property_dependency_chain_ILineItem> ();
            //order.LineItems.Add (li);
            //myViewModel.Orders.Add (order);
            myExpectedNotifications.Add ("TotalOrders");
            myExpectedNotifications.Add ("TotalLineItems");
            myExpectedNotifications.Add ("TotalItemQuantity");
            myExpectedNotifications.Add ("TotalCost");
            //myViewModel.Orders[1].LineItems[0].Quantity = 3;
            myExpectedNotifications.Add ("TotalItemQuantity");
            myExpectedNotifications.Add ("TotalCost");
        }

        protected virtual void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
        { myActualNotifications.Add (e.PropertyName); }

        protected override void Because_of ()
        {
            try
            {
                myViewModel.Orders = new ObservableCollection<when_testing_collection_deep_IOrder> ();
                // Orders
                // TotalOrders
                // TotalLineItems
                // TotalItemQuantity
                // TotalCost
                myViewModel.Orders.Add (Activator.CreateInstance<TOrder> ());
                // TotalOrders
                // TotalLineItems
                // TotalItemQuantity
                // TotalCost
                myViewModel.Orders[0].LineItems = new ObservableCollection<when_testing_2deep_property_dependency_chain_ILineItem> ();
                // TotalLineItems
                // TotalItemQuantity
                // TotalCost
                myViewModel.Orders[0].LineItems.Add (Activator.CreateInstance<TLineItem> ());
                // TotalLineItems
                // TotalItemQuantity
                // TotalCost
                myViewModel.Orders[0].LineItems[0].Quantity = 1;
                // TotalItemQuantity
                // TotalCost
                myViewModel.Orders[0].LineItems[0].Price = 100;
                // TotalCost
                var li = Activator.CreateInstance<TLineItem> ();
                li.Quantity = 2;
                li.Price = 50;
                myViewModel.Orders[0].LineItems.Add (li);
                // TotalLineItems
                // TotalItemQuantity
                // TotalCost
                var order = Activator.CreateInstance<TOrder> ();
                li = Activator.CreateInstance<TLineItem> ();
                li.Quantity = 2;
                li.Price = 33.33m;
                order.LineItems = new ObservableCollection<when_testing_2deep_property_dependency_chain_ILineItem> ();
                order.LineItems.Add (li);
                myViewModel.Orders.Add (order);
                // TotalOrders
                // TotalLineItems
                // TotalItemQuantity
                // TotalCost
                myViewModel.Orders[1].LineItems[0].Quantity = 3;
                // TotalItemQuantity
                // TotalCost
            }
            catch (Exception ex)
            {
                m_BecauseOfException = ex;
            }
        }
    }

    /// <summary>
    /// Order History
    /// </summary>
    public interface when_testing_collection_deep_IViewModel : INotifyPropertyChanged
    {
        ObservableCollection<when_testing_collection_deep_IOrder> Orders { get; set; }

        int TotalOrders { get; }
        int TotalLineItems { get; }
        int TotalItemQuantity { get; }
        decimal TotalCost { get; }
    }

    public interface when_testing_collection_deep_IOrder : INotifyPropertyChanged
    {
        ObservableCollection<when_testing_2deep_property_dependency_chain_ILineItem> LineItems { get; set; }

        int TotalLineItems { get; }
        int TotalItemQuantity { get; }
        decimal TotalCost { get; }
    }

    public class when_using_MvvmNotificationChainer_and_collection_deep :
        when_testing_collection_deep<
            when_using_MvvmNotificationChainer_and_collection_deep_ViewModel,
            when_using_MvvmNotificationChainer_and_collection_deep_Order,
            when_using_MvvmNotificationChainer_and_collection_deep_LineItem>
    {
        protected override void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PropertyChangedOutput") return;
            base.OnPropertyChanged (sender, e);
        }
    }

    public class when_using_MvvmNotificationChainer_and_collection_deep_ViewModel : NotifyPropertyChangedBase, when_testing_collection_deep_IViewModel
    {
        private ObservableCollection<when_testing_collection_deep_IOrder> myOrders;
        public ObservableCollection<when_testing_collection_deep_IOrder> Orders
        {
            get { return myOrders; }
            set
            {
                myOrders = value;
                RaisePropertyChanged ();
            }
        }

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

    public class when_using_MvvmNotificationChainer_and_collection_deep_Order : NotifyPropertyChangedBase, when_testing_collection_deep_IOrder
    {
        private ObservableCollection<when_testing_2deep_property_dependency_chain_ILineItem> myLineItems;
        public ObservableCollection<when_testing_2deep_property_dependency_chain_ILineItem> LineItems
        {
            get { return myLineItems; }
            set
            {
                myLineItems = value;
                RaisePropertyChanged ();
            }
        }

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

    public class when_using_MvvmNotificationChainer_and_collection_deep_LineItem : when_using_NotificationChainPropertyAttribute_LineItem
    {
    }
}
