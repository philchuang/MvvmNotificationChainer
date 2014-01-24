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

namespace Demo3
{
    public class MainWindowViewModel : NotifyPropertyChangedBaseDebug
    {
        private Manager myManager1;
        public Manager Manager1
        {
            get { return myManager1; }
            set
            {
                myManager1 = value;
                RaisePropertyChanged ();
            }
        }

        public String Manager1Name
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                                          .Configure (cn => cn.On (() => Manager1,
                                                                   m => m.Name)
                                                              .Finish ());

                return myManager1 == null ? "N/A" : myManager1.Name;
            }
        }

        [CommandProperty (commandType: typeof (DelegateCommand))]
        public ICommand Manager1Command
        { get; private set; }

        [CommandExecuteMethod]
        private void ExecuteManager1 ()
        {
            if (Manager1 == null)
                Manager1 = new Manager();
            else
                Manager1 = null;
        }

        public String Manager1CommandText
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                                          .Configure (cn => cn.On (() => Manager1).Finish ());

                return Manager1 == null ? "+" : "x";
            }
        }

        [CommandProperty (commandType: typeof (DelegateCommand))]
        public DelegateCommand Manager1Employee1Command
        { get; private set; }

        [CommandCanExecuteMethod]
        private bool CanManager1Employee1 ()
        {
            myNotificationChainManager.CreateOrGet (() => Manager1Employee1Command)
                                      .Configure (cn => cn.AndClearCalls ()
                                                          .On (() => Manager1)
                                                          .AndCall (Manager1Employee1Command.RaiseCanExecuteChanged)
                                                          .Finish ());

            return myManager1 != null;
        }

        [CommandExecuteMethod]
        private void ExecuteManager1Employee1 ()
        {
            if (!CanManager1Employee1 ()) return;

            if (Manager1.Employee1 == null)
                Manager1.Employee1 = new Employee ();
            else
                Manager1.Employee1 = null;
        }

        public String Manager1Employee1CommandText
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                                          .Configure (cn => cn.On (() => Manager1,
                                                                   m => m.Employee1)
                                                              .Finish ());

                return Manager1 == null ? "" : Manager1.Employee1 == null ? "+" : "x";
            }
        }

        public String Manager1Employee1Name
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                                          .Configure (cn => cn.On (() => Manager1,
                                                                   m => m.Employee1,
                                                                   e => e.Name)
                                                              .Finish ());

                return myManager1 == null || myManager1.Employee1 == null ? "N/A" : myManager1.Employee1.Name;
            }
        }

        [CommandProperty (commandType: typeof (DelegateCommand))]
        public DelegateCommand Manager1Employee2Command
        { get; private set; }

        [CommandCanExecuteMethod]
        private bool CanManager1Employee2 ()
        {
            myNotificationChainManager.CreateOrGet (() => Manager1Employee2Command)
                                      .Configure (cn => cn.AndClearCalls ()
                                                          .On (() => Manager1)
                                                          .AndCall (Manager1Employee2Command.RaiseCanExecuteChanged)
                                                          .Finish ());

            return myManager1 != null;
        }

        [CommandExecuteMethod]
        private void ExecuteManager1Employee2 ()
        {
            if (!CanManager1Employee2 ()) return;

            if (Manager1.Employee2 == null)
                Manager1.Employee2 = new Employee ();
            else
                Manager1.Employee2 = null;
        }

        public String Manager1Employee2CommandText
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                                          .Configure (cn => cn.On (() => Manager1,
                                                                   m => m.Employee2)
                                                              .Finish ());

                return Manager1 == null ? "" : Manager1.Employee2 == null ? "+" : "x";
            }
        }

        public String Manager1Employee2Name
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                                          .Configure (cn => cn.On (() => Manager1,
                                                                   m => m.Employee2,
                                                                   e => e.Name)
                                                              .Finish ());

                return myManager1 == null || myManager1.Employee2 == null ? "N/A" : myManager1.Employee2.Name;
            }
        }
        
        private Manager myManager2;
        public Manager Manager2
        {
            get { return myManager2; }
            set
            {
                myManager2 = value;
                RaisePropertyChanged ();
            }
        }

        public String Manager2Name
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                                          .Configure (cn => cn.On (() => Manager2,
                                                                   m => m.Name)
                                                              .Finish ());

                return myManager2 == null ? "N/A" : myManager2.Name;
            }
        }

        [CommandProperty (commandType: typeof (DelegateCommand))]
        public ICommand Manager2Command
        { get; private set; }

        [CommandExecuteMethod]
        private void ExecuteManager2 ()
        {
            if (Manager2 == null)
                Manager2 = new Manager();
            else
                Manager2 = null;
        }

        public String Manager2CommandText
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                                          .Configure (cn => cn.On (() => Manager2).Finish ());

                return Manager2 == null ? "+" : "x";
            }
        }

        [CommandProperty (commandType: typeof (DelegateCommand))]
        public DelegateCommand Manager2Employee1Command
        { get; private set; }

        [CommandCanExecuteMethod]
        private bool CanManager2Employee1 ()
        {
            myNotificationChainManager.CreateOrGet (() => Manager2Employee1Command)
                                      .Configure (cn => cn.AndClearCalls ()
                                                          .On (() => Manager2)
                                                          .AndCall (Manager2Employee1Command.RaiseCanExecuteChanged)
                                                          .Finish ());

            return myManager2 != null;
        }

        [CommandExecuteMethod]
        private void ExecuteManager2Employee1 ()
        {
            if (!CanManager2Employee1 ()) return;

            if (Manager2.Employee1 == null)
                Manager2.Employee1 = new Employee ();
            else
                Manager2.Employee1 = null;
        }

        public String Manager2Employee1CommandText
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                                          .Configure (cn => cn.On (() => Manager2,
                                                                   m => m.Employee1)
                                                              .Finish ());

                return Manager2 == null ? "" : Manager2.Employee1 == null ? "+" : "x";
            }
        }

        public String Manager2Employee1Name
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                                          .Configure (cn => cn.On (() => Manager2,
                                                                   m => m.Employee1,
                                                                   e => e.Name)
                                                              .Finish ());

                return myManager2 == null || myManager2.Employee1 == null ? "N/A" : myManager2.Employee1.Name;
            }
        }

        [CommandProperty (commandType: typeof (DelegateCommand))]
        public DelegateCommand Manager2Employee2Command
        { get; private set; }

        [CommandCanExecuteMethod]
        private bool CanManager2Employee2 ()
        {
            myNotificationChainManager.CreateOrGet (() => Manager2Employee2Command)
                                      .Configure (cn => cn.AndClearCalls ()
                                                          .On (() => Manager2)
                                                          .AndCall (Manager2Employee2Command.RaiseCanExecuteChanged)
                                                          .Finish ());

            return myManager2 != null;
        }

        [CommandExecuteMethod]
        private void ExecuteManager2Employee2 ()
        {
            if (!CanManager2Employee2 ()) return;

            if (Manager2.Employee2 == null)
                Manager2.Employee2 = new Employee ();
            else
                Manager2.Employee2 = null;
        }

        public String Manager2Employee2CommandText
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                                          .Configure (cn => cn.On (() => Manager2,
                                                                   m => m.Employee2)
                                                              .Finish ());

                return Manager2 == null ? "" : Manager2.Employee2 == null ? "+" : "x";
            }
        }

        public String Manager2Employee2Name
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                                          .Configure (cn => cn.On (() => Manager2,
                                                                   m => m.Employee2,
                                                                   e => e.Name)
                                                              .Finish ());

                return myManager2 == null || myManager2.Employee2 == null ? "N/A" : myManager2.Employee2.Name;
            }
        }
        
        public MainWindowViewModel ()
        {
            CommandWirer.WireAll (this);
        }
    }

    public class Employee : NotifyPropertyChangedBase
    {
        private String myName;
        public String Name
        {
            get { return myName; }
            set
            {
                myName = value;
                RaisePropertyChanged();
            }
        }
    }

    public class Manager : Employee
    {
        private Employee myEmployee1;
        public Employee Employee1
        {
            get { return myEmployee1; }
            set
            {
                myEmployee1 = value;
                RaisePropertyChanged ();
            }
        }

        private Employee myEmployee2;
        public Employee Employee2
        {
            get { return myEmployee2; }
            set
            {
                myEmployee2 = value;
                RaisePropertyChanged ();
            }
        }
    }
}