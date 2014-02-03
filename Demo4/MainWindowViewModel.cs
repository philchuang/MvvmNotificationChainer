using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Com.PhilChuang.Utils.MvvmCommandWirer;
using Com.PhilChuang.Utils.MvvmNotificationChainer;
using Demo.Utils;
using Microsoft.Practices.Prism.Commands;

namespace Demo4
{
    /// <summary>
    /// This class represents an Order History
    /// </summary>
    public class MainWindowViewModel : NotifyPropertyChangedBaseDebug
    {
        private ObservableCollection<Order> myOrders;
        public ObservableCollection<Order> Orders
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
        public int TotalItems
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                                          .Configure (cn => cn.OnCollection (() => Orders, o => o.TotalItems).Finish ());

                return Orders == null ? 0 : Orders.Select (o => o.TotalItems).Sum ();
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

    public class Order : NotifyPropertyChangedBase
    {
        private ObservableCollection<LineItem> myLineItems;
        public ObservableCollection<LineItem> LineItems
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

                return LineItems == null ? 0 : LineItems.Count;
            }
        }

        [NotificationChainProperty]
        public int TotalItems
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                                          .Configure (cn => cn.OnCollection (() => LineItems, li => li.Quantity).Finish ());

                return LineItems == null ? 0 : LineItems.Select (li => li.Quantity).Sum ();
            }
        }

        [NotificationChainProperty]
        public decimal TotalCost
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                                          .Configure (cn => cn.OnCollection (() => LineItems, li => li.Cost).Finish ());

                return LineItems == null ? 0 : LineItems.Select (li => li.Cost).Sum ();
            }
        }
    }

    public class LineItem : NotifyPropertyChangedBase
    {
        private int myQuantity;
        public int Quantity
        {
            get { return myQuantity; }
            set
            {
                myQuantity = value;
                RaisePropertyChanged ();
            }
        }

        private decimal myPrice;
        public decimal Price
        {
            get { return myPrice; }
            set
            {
                myPrice = value;
                RaisePropertyChanged ();
            }
        }

        [NotificationChainProperty]
        public decimal Cost
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                                          .Configure (cn => cn.On (() => Quantity)
                                                              .On (() => Price)
                                                              .Finish ());

                return Quantity * Price;
            }
        }
    }
}