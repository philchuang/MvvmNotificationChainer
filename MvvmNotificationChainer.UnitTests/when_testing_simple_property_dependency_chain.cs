using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Demo.Utils;
using JetBrains.Annotations;

// ReSharper disable InconsistentNaming
namespace MvvmNotificationChainer.UnitTests
{
    public abstract class when_testing_simple_property_dependency_chain<TViewModel> : when_using_INotifyPropertyChanged
        where TViewModel : when_testing_simple_property_dependency_chain<TViewModel>.IViewModel
    {
        public interface IViewModel : INotifyPropertyChanged
        {
            /// <summary>
            /// Source property, item quantity
            /// </summary>
            int Quantity { get; set; }

            /// <summary>
            /// Source property, individual item price
            /// </summary>
            decimal Price { get; set; }

            /// <summary>
            /// Derived property, item quantity * individual item price
            /// </summary>
            decimal Cost { get; }
        }

        protected TViewModel myViewModel;

        protected override void Establish_context ()
        {
            myViewModel = Activator.CreateInstance<TViewModel> ();
            myViewModel.PropertyChanged += OnPropertyChanged;

            myExpectedNotifications.Add ("Quantity");
            myExpectedNotifications.Add ("Cost");
            myExpectedNotifications.Add ("Price");
            myExpectedNotifications.Add ("Cost");
        }

        protected virtual void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
        { myActualNotifications.Add (e.PropertyName); }

        protected override void Because_of ()
        {
            try
            {
                myViewModel.Quantity = 1;
                myViewModel.Price = 99.99m;
            }
            catch (Exception ex)
            {
                m_BecauseOfException = ex;
            }
        }
    }

    public class when_not_using_NotificationChain_and_testing_simple_chain : 
        when_testing_simple_property_dependency_chain<when_not_using_NotificationChain_and_testing_simple_chain.ViewModel>
    {
        public class ViewModel : NotifyPropertyChangedBase, IViewModel
        {
            private int myQuantity;
            public int Quantity
            {
                get { return myQuantity; }
                set
                {
                    myQuantity = value;
                    RaisePropertyChanged ();
                    RaisePropertyChanged (() => Cost);
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
                    RaisePropertyChanged (() => Cost);
                }
            }

            public decimal Cost
            { get { return Quantity * Price; } }
        }

        protected override void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PropertyChangedOutput") return;
            base.OnPropertyChanged (sender, e);
        }
    }

    public class when_using_NotificationChain_and_testing_simple_chain :
        when_testing_simple_property_dependency_chain<when_using_NotificationChain_and_testing_simple_chain.ViewModel>
    {
        public class ViewModel : NotifyPropertyChangedBase, IViewModel
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

                    return Quantity * Price;
                }
            }
        }

        protected override void Because_of ()
        {
            try
            {
                // call dependent properties to initialize the chain
                var cost = myViewModel.Cost;
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
}
