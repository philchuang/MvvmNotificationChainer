using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Com.PhilChuang.Utils;
using Com.PhilChuang.Utils.MvvmCommandWirer;
using Com.PhilChuang.Utils.MvvmNotificationChainer;
using Demo.Utils;
using Microsoft.Practices.Prism.Commands;

namespace Demo
{
    public class MainWindowViewModel : NotifyPropertyChangedBaseDebug
    {
        // ----- the old way of doing chained properties ------------------------------------------
        // ----- note that the "parent" properties (Int1 and Int2) have to know about its dependent property (IntSum)

        private int myExample1Int1;
        public int Example1Int1
        {
            get { return myExample1Int1; }
            set
            {
                myExample1Int1 = value;
                RaisePropertyChanged ();
                RaisePropertyChanged (() => Example1IntSum);
            }
        }

        private int myExample1Int2;
        public int Example1Int2
        {
            get { return myExample1Int2; }
            set
            {
                myExample1Int2 = value;
                RaisePropertyChanged ();
                RaisePropertyChanged (() => Example1IntSum);
            }
        }

        public int Example1IntSum
        {
            get { return myExample1Int1 + myExample1Int2; }
        }

        // ----- the new way of doing chained properties ------------------------------------------
        // ----- note that the "parent" properties (Int1 and Int2) DO NOT know about its dependent property (IntSum)
        // ----- the dependent property (IntSum) is now responsible for the knowledge that it depends on Int1 and Int2

        private int myExample2Int1;
        public int Example2Int1
        {
            get { return myExample2Int1; }
            set
            {
                myExample2Int1 = value;
                RaisePropertyChanged ();
            }
        }

        private int myExample2Int2;
        public int Example2Int2
        {
            get { return myExample2Int2; }
            set
            {
                myExample2Int2 = value;
                RaisePropertyChanged ();
            }
        }

        [NotificationChainProperty]
        public int Example2IntSum
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                                          .Configure (cn => cn.On (() => Example2Int1)
                                                              .On (() => Example2Int2)
                                                              .Finish ());

                return myExample2Int1 + myExample2Int2;
            }
        }

        // ----- the new way of doing chained properties ------------------------------------------
        // ----- this variation allows any PropertyChangedEventHandler to be chained (not just INotifyPropertyChanged.PropertyChanged)

        private int myExample3Int1;
        public int Example3Int1
        {
            get { return myExample3Int1; }
            set
            {
                myExample3Int1 = value;
                RaisePropertyChanged ();
            }
        }

        private int myExample3Int2;
        public int Example3Int2
        {
            get { return myExample3Int2; }
            set
            {
                myExample3Int2 = value;
                RaisePropertyChanged ();
            }
        }

        [NotificationChainProperty]
        public int Example3IntSum
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                                          .Configure (cn => cn.On (() => Example3Int1)
                                                              .On (() => Example3Int2)
                                                              .Finish ());

                return myExample3Int1 + myExample3Int2;
            }
        }

        // ----- sub-property chaining ------------------------------------------
        // ----- demonstrates chaining on a Property's Property

        private static readonly String[] s_Example4CommandText =
        {
            "Create Randomizer",
            "Randomize()",
            "Clear Randomizer",
        };

        private int myExample4CommandTextIndex = 0;
        private int Example4CommandTextIndex
        {
            get { return myExample4CommandTextIndex; }
            set
            {
                myExample4CommandTextIndex = value;
                RaisePropertyChangedInternal ();
            }
        }

        [NotificationChainProperty]
        public String Example4CommandText
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                                          .Configure (cn => cn.On (() => Example4CommandTextIndex).Finish ());

                return s_Example4CommandText[Example4CommandTextIndex];
            }
        }

        [CommandProperty (commandType: typeof (DelegateCommand))]
        public ICommand Example4Command { get; set; }

        [CommandExecuteMethod]
        private void ExecuteExample4 ()
        {
            if (myExample4CommandTextIndex == 0)
                Example4Randomizer = new RandomIntGenerator ();
            else if (myExample4CommandTextIndex == 1)
                Example4Randomizer.Randomize ();
            else if (myExample4CommandTextIndex == 2)
                Example4Randomizer = null;

            Example4CommandTextIndex = (Example4CommandTextIndex + 1) % s_Example4CommandText.Length;
        }

        private RandomIntGenerator myExample4Randomizer;
        public RandomIntGenerator Example4Randomizer
        {
            get { return myExample4Randomizer; }
            set
            {
                myExample4Randomizer = value;
                RaisePropertyChanged ();
            }
        }

        [NotificationChainProperty]
        public int Example4Int
        {
            get
            {
                // notify when Example4Randomizer and Example4Randomizer.Int changes
                myNotificationChainManager.CreateOrGet ()
                                          .Configure (cn => cn.On (() => Example4Randomizer, rig => rig.Int)
                                                              .Finish ());

                return Example4Randomizer != null ? Example4Randomizer.Int : -1;
            }
        }

        // ----- command chaining ------------------------------------------

        private int myExample5Int;
        public int Example5Int
        {
            get { return myExample5Int; }
            set
            {
                myExample5Int = value;
                RaisePropertyChanged ();
            }
        }

        [CommandProperty (commandType: typeof (DelegateCommand<int?>), paramType: typeof (int?))]
        public DelegateCommand<int?> Example5Command
        { get; private set; }

        [CommandCanExecuteMethod]
        private bool CanExample5 (int? parameter)
        {
            myNotificationChainManager.CreateOrGet (() => Example5Command)
                                      .Configure (cn =>
                                                  cn.AndClearCalls ()
                                                    .On (() => Example5Int)
                                                    .AndCall (Example5Command.RaiseCanExecuteChanged)
                                                    .Finish ());

            return parameter != null && parameter.Value % 2 == 1;
        }

        [CommandExecuteMethod]
        private void DoExample5 (int? parameter)
        {
            Example5Int = (parameter ?? 1) + 1;
        }

        public MainWindowViewModel ()
        {
            CommandWirer.WireAll (this);
        }
    }

    public class RandomIntGenerator : NotifyPropertyChangedBaseDebug
    {
        private int myInt;
        public int Int
        {
            get { return myInt; }
            set
            {
                myInt = value;
                RaisePropertyChanged ();
            }
        }

        public RandomIntGenerator ()
        {
            Int = 0;
        }

        public void Randomize ()
        {
            Int = new Random ().Next ();
        }
    }
}