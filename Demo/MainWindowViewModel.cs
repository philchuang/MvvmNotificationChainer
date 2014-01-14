using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Demo.Utils;

namespace Demo
{
    public class MainWindowViewModel : ViewModelBase
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
                RaisePropertyChanged();
                RaisePropertyChanged(() => Example1IntSum);
            }
        }

        private int myExample1Int2;
        public int Example1Int2
        {
            get { return myExample1Int2; }
            set
            {
                myExample1Int2 = value;
                RaisePropertyChanged();
                RaisePropertyChanged(() => Example1IntSum);
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
                RaisePropertyChanged();
            }
        }

        private int myExample2Int2;
        public int Example2Int2
        {
            get { return myExample2Int2; }
            set
            {
                myExample2Int2 = value;
                RaisePropertyChanged();
            }
        }

        public int Example2IntSum
        {
            get
            {
                myChainedNotifications.Create()
                                      .Register (cnd => cnd.On (this, () => Example2Int1)
                                                           .On (this, () => Example2Int2)
                                                           .AndCall (RaisePropertyChanged))
                                      .Finish ();

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
                RaisePropertyChanged();
            }
        }

        private int myExample3Int2;
        public int Example3Int2
        {
            get { return myExample3Int2; }
            set
            {
                myExample3Int2 = value;
                RaisePropertyChanged();
            }
        }

        public int Example3IntSum
        {
            get
            {
                CreateChain ()
                    .Register (cnd => cnd.On (() => Example3Int1)
                                         .On (() => Example3Int2))
                    .Finish ();

                return myExample3Int1 + myExample3Int2;
            }
        }
    }
}