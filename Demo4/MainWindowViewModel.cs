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
        private ObservableCollection<Order> myOrders = new ObservableCollection<Order> ();
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

        [CommandProperty (commandType: typeof (DelegateCommand))]
        public ICommand AddOrderCommand { get; private set; }

        [CommandExecuteMethod]
        private void AddOrder ()
        {
            Orders.Add (new Order { Id = Orders.Any () ? Orders.Select (o => o.Id).Max () + 1 : 1 });
        }

        [CommandProperty (commandType: typeof (DelegateCommand<Order>), paramType: typeof (Order))]
        public ICommand DeleteOrderCommand { get; private set; }

        [CommandCanExecuteMethod]
        private bool CanDeleteOrder (Order order)
        { return order != null; }

        [CommandExecuteMethod]
        private void DeleteOrder (Order order)
        {
            Orders.Remove (order);
        }

        [CommandProperty (commandType: typeof (DelegateCommand<Order>), paramType: typeof (Order))]
        public ICommand AddLineItemCommand { get; private set; }

        [CommandCanExecuteMethod]
        private bool CanAddLineItem (Order order)
        { return order != null; }

        [CommandExecuteMethod]
        private void AddLineItem (Order order)
        {
            order.LineItems.Add (new LineItem { Index = order.LineItems.Any () ? order.LineItems.Select (o => o.Index).Max () + 1 : 1, Order = order });
        }

        [CommandProperty (commandType: typeof (DelegateCommand<LineItem>), paramType: typeof (LineItem))]
        public ICommand DeleteLineItemCommand { get; private set; }

        [CommandCanExecuteMethod]
        private bool CanDeleteLineItem (LineItem item)
        { return item != null; }

        [CommandExecuteMethod]
        private void DeleteLineItem (LineItem item)
        {
            item.Order.LineItems.Remove (item);
        }

        public MainWindowViewModel ()
        {
            CommandWirer.WireAll (this);
        }
    }

    public class Order : NotifyPropertyChangedBase
    {
        private int myId;
        public int Id
        {
            get { return myId; }
            set
            {
                myId = value;
                RaisePropertyChanged ();
            }
        }

        private ObservableCollection<LineItem> myLineItems = new ObservableCollection<LineItem> ();
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
        private int myIndex;
        public int Index
        {
            get { return myIndex; }
            set
            {
                myIndex = value;
                RaisePropertyChanged ();
            }
        }

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

        private Order myOrder;
        public Order Order
        {
            get { return myOrder; }
            set
            {
                myOrder = value;
                RaisePropertyChanged ();
            }
        }
    }
}