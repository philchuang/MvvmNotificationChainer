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
using Demo.Utils;
using JetBrains.Annotations;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace MvvmNotificationChainer.UnitTests
{
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
                    // TODO see about eliminating explicit type parameters
                                          .Configure (cn => cn.OnCollection
                                                                <ObservableCollection<when_testing_2deep_property_dependency_chain_ILineItem>,
                                                                when_testing_2deep_property_dependency_chain_ILineItem>
                                                                (() => LineItems)
                                                              .Finish ());

                return myLineItems != null ? myLineItems.Count : 0;
            }
        }

        public int TotalItemQuantity
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                    // TODO see about eliminating explicit type parameters
                                          .Configure (cn => cn.OnCollection
                                                                <ObservableCollection<when_testing_2deep_property_dependency_chain_ILineItem>,
                                                                when_testing_2deep_property_dependency_chain_ILineItem,
                                                                int>
                                                                (() => LineItems, li => li.Quantity)
                                                              .Finish ());

                return myLineItems != null ? myLineItems.Select (li => li.Quantity).Sum () : 0;
            }
        }

        public decimal TotalCost
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                    // TODO see about eliminating explicit type parameters
                                          .Configure (cn => cn.OnCollection
                                                                <ObservableCollection<when_testing_2deep_property_dependency_chain_ILineItem>,
                                                                when_testing_2deep_property_dependency_chain_ILineItem,
                                                                decimal>
                                                                (() => LineItems, li => li.Cost)
                                                              .Finish ());

                return myLineItems != null ? myLineItems.Select (li => li.Cost).Sum () : 0;
            }
        }
    }

    public class when_using_MvvmNotificationChainer_and_collection_LineItem : when_using_NotificationChainPropertyAttribute_LineItem
    {
    }
}
