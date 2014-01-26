using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Com.PhilChuang.Utils.MvvmCommandWirer;
using Demo.Utils;
using Microsoft.Practices.Prism.Commands;

namespace Demo2
{
    public class MainWindowViewModel : NotifyPropertyChangedBaseDebug<MainWindowViewModel>
    {
        private LineItem myLineItem1;
        public LineItem LineItem1
        {
            get { return myLineItem1; }
            set
            {
                myLineItem1 = value;
                RaisePropertyChanged ();
            }
        }

        public String LineItem1CommandText
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                                          .Configure (cn => cn.On (() => LineItem1).Finish ());

                return myLineItem1 == null ? "+" : "x";
            }
        }

        [CommandProperty (commandType: typeof (DelegateCommand))]
        public ICommand LineItem1Command { get; set; }

        [CommandExecuteMethod]
        private void ExecuteLineItem1 ()
        {
            LineItem1 = LineItem1 == null ? new LineItem () : null;
        }

        private LineItem myLineItem2;
        public LineItem LineItem2
        {
            get { return myLineItem2; }
            set
            {
                myLineItem2 = value;
                RaisePropertyChanged ();
            }
        }

        public String LineItem2CommandText
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                                          .Configure (cn => cn.On (() => LineItem2).Finish ());

                return myLineItem2 == null ? "+" : "x";
            }
        }

        [CommandProperty (commandType: typeof (DelegateCommand))]
        public ICommand LineItem2Command { get; set; }

        [CommandExecuteMethod]
        private void ExecuteLineItem2 ()
        {
            LineItem2 = LineItem2 == null ? new LineItem () : null;
        }

        private LineItem myLineItem3;
        public LineItem LineItem3
        {
            get { return myLineItem3; }
            set
            {
                myLineItem3 = value;
                RaisePropertyChanged ();
            }
        }

        public String LineItem3CommandText
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                                          .Configure (cn => cn.On (() => LineItem3).Finish ());

                return myLineItem3 == null ? "+" : "x";
            }
        }

        [CommandProperty (commandType: typeof (DelegateCommand))]
        public ICommand LineItem3Command { get; set; }

        [CommandExecuteMethod]
        private void ExecuteLineItem3 ()
        {
            LineItem3 = LineItem3 == null ? new LineItem () : null;
        }

        public decimal TotalCost
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                                          .Configure (cn => cn.On (() => LineItem1, li => li.Cost)
                                                              .On (() => LineItem2, li => li.Cost)
                                                                // doing multiple LineItem3 properties just to show that it can be done
                                                              .On (() => LineItem3, li => li.Quantity)
                                                              .On (() => LineItem3, li => li.Price)
                                                              .Finish ());

                return (LineItem1 != null ? LineItem1.Cost : 0m) +
                       (LineItem2 != null ? LineItem2.Cost : 0m) +
                       (LineItem3 != null ? LineItem3.Quantity * LineItem3.Price : 0m);
            }
        }

        public int TotalQuantity
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                                          .Configure (cn => cn.On (() => LineItem1, li => li.Quantity)
                                                              .On (() => LineItem2, li => li.Quantity)
                                                              .On (() => LineItem3, li => li.Quantity)
                                                              .Finish ());

                return (LineItem1 != null ? LineItem1.Quantity : 0) +
                       (LineItem2 != null ? LineItem2.Quantity : 0) +
                       (LineItem3 != null ? LineItem3.Quantity : 0);
            }
        }

        public int NumLineItems
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                                          .Configure (cn => cn.On (() => LineItem1)
                                                              .On (() => LineItem2)
                                                              .On (() => LineItem3)
                                                              .Finish ());

                return (LineItem1 != null ? 1 : 0) +
                       (LineItem2 != null ? 1 : 0) +
                       (LineItem3 != null ? 1 : 0);
            }
        }

        public MainWindowViewModel ()
        {
            CommandWirer.WireAll (this);
        }
    }

    public class LineItem : NotifyPropertyChangedBase<LineItem>
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

        public decimal Cost
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                                          .Configure (cn => cn.On (() => Quantity)
                                                              .On (() => Price)
                                                              .Finish ());

                return myQuantity * myPrice;
            }
        }
    }
}