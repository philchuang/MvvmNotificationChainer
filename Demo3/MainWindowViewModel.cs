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
    public class MainWindowViewModel : NotifyPropertyChangedBase
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

        public String Manager1CommandText
        {
            get
            {
                return Manager1 == null
                           ? "+"
                           : "x";
            }
        }

        [CommandProperty (commandType: typeof(DelegateCommand))]
        public ICommand Manager1Command
        { get; private set; }

        [CommandExecuteMethod]
        private void ExecuteManager1 ()
        {
            if (Manager1 == null)
            {
                Manager1 = new Manager();
            }
            else
            {
                Manager1 = null;
            }
            RaisePropertyChanged (() => Manager1CommandText);
            RaisePropertyChanged (() => Employee1CommandText);
            ((DelegateCommand) Employee1Command).RaiseCanExecuteChanged ();
        }

        public String Employee1CommandText
        {
            get
            {
                return Manager1 == null
                           ? ""
                           : Manager1.Employee1 == null
                                 ? "+"
                                 : "x";
            }
        }
        [CommandProperty (commandType: typeof (DelegateCommand))]
        public ICommand Employee1Command
        { get; private set; }

        [CommandCanExecuteMethod]
        private bool CanEmployee1 ()
        {
            return myManager1 != null;
        }

        [CommandExecuteMethod]
        private void ExecuteEmployee1 ()
        {
            if (!CanEmployee1 ()) return;

            if (Manager1.Employee1 == null)
            {
                Manager1.Employee1 = new Employee ();
            }
            else
            {
                Manager1.Employee1 = null;
            }
            RaisePropertyChanged(() => Employee1CommandText);
        }

        public String Manager1Employee1Name
        {
            get
            {
                myNotificationChainManager.CreateOrGet ()
                                          .Register (cn => cn.On (() => Manager1,
                                                                  m => m.Employee1,
                                                                  e => e.Name)
                                                             .Finish ());

                return myManager1 == null || myManager1.Employee1 == null ? "N/A" : myManager1.Employee1.Name;
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
    }
}