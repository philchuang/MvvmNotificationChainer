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
    public class MainWindowViewModel : NotifyPropertyChangedBase
    {
        // TODO chain LineItemNCommandText to ExecuteLineItemN? Perhaps DelegateCommand.IsActiveChanged can trigger

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
        { get { return myLineItem1 == null ? "+" : "x"; } }

        [CommandProperty (commandType: typeof (DelegateCommand))]
        public ICommand LineItem1Command { get; set; }

        [CommandExecuteMethod]
        private void ExecuteLineItem1 ()
        {
            LineItem1 = LineItem1 == null ? new LineItem () : null;
            RaisePropertyChanged (() => LineItem1CommandText);
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
        { get { return myLineItem2 == null ? "+" : "x"; } }

        [CommandProperty (commandType: typeof (DelegateCommand))]
        public ICommand LineItem2Command { get; set; }

        [CommandExecuteMethod]
        private void ExecuteLineItem2 ()
        {
            LineItem2 = LineItem2 == null ? new LineItem () : null;
            RaisePropertyChanged (() => LineItem2CommandText);
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
        { get { return myLineItem3 == null ? "+" : "x"; } }

        [CommandProperty (commandType: typeof (DelegateCommand))]
        public ICommand LineItem3Command { get; set; }

        [CommandExecuteMethod]
        private void ExecuteLineItem3 ()
        {
            LineItem3 = LineItem3 == null ? new LineItem () : null;
            RaisePropertyChanged (() => LineItem3CommandText);
        }

        public decimal TotalCost
        {
            get
            {
                CreateChain ()
                    .Register (cn => cn.On (() => LineItem1, li => li.Cost)
                                       .On (() => LineItem2, li => li.Cost)
                                       .On (() => LineItem3, li => li.Cost)
                                       .Finish ());

                return (LineItem1 != null ? LineItem1.Cost : 0m) +
                       (LineItem2 != null ? LineItem2.Cost : 0m) +
                       (LineItem3 != null ? LineItem3.Cost : 0m);
            }
        }

        public int NumLineItems
        {
            get
            {
                CreateChain ()
                    .Register (cn => cn.On (() => LineItem1)
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

        public decimal Cost
        {
            get
            {
                CreateChain ()
                    .Register (cn => cn.On (() => Quantity)
                                       .On (() => Price)
                                       .Finish ());

                return myQuantity * myPrice;
            }
        }
    }
}