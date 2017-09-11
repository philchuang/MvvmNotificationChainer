using System.Collections.Generic;
using System.ComponentModel;
using Com.PhilChuang.Utils.MvvmNotificationChainer;
using Demo.Utils;
using Xunit;

namespace MvvmNotificationChainer.UnitTests
{
    public class DeepNotificationTests : NotificationTestBase
    {
        [Theory]
        [MemberData(nameof(TestCases_for_ViewModel_with_nested_properties_should_notify))]
        public void ViewModel_with_nested_properties_should_notify(ITotalLineItemsViewModel viewModel)
        {
            // ARRANGE
            viewModel.PropertyChanged += (_, e) => ActualNotifications.Add(e.PropertyName);

            ExpectedNotifications.AddRange(new[] {
                //viewModel.LineItem1 = viewModel.CreateLineItem1();
                "LineItem1",
                "TotalLineItems",
                "TotalItemQuantity",
                "TotalCost",
                //viewModel.LineItem1.Quantity = 1;
                "TotalItemQuantity",
                "TotalCost",
                //viewModel.LineItem1.Price = 99.99m;
                "TotalCost",
                //viewModel.LineItem2 = viewModel.CreateLineItem2();
                "LineItem2",
                "TotalLineItems",
                "TotalItemQuantity",
                "TotalCost",
                //viewModel.LineItem2.Quantity = 2;
                "TotalItemQuantity",
                "TotalCost",
                //viewModel.LineItem2.Price = 50.00m;
                "TotalCost",
                //viewModel.LineItem3 = viewModel.CreateLineItem3();
                "LineItem3",
                "TotalLineItems",
                "TotalItemQuantity",
                "TotalCost",
                //viewModel.LineItem3 = null;
                "LineItem3",
                "TotalLineItems",
                "TotalItemQuantity",
                "TotalCost",
            });

            // ACT
            viewModel.CreateLineItem1();
            viewModel.LineItem1.Quantity = 1;
            viewModel.LineItem1.Price = 99.99m;
            viewModel.CreateLineItem2();
            viewModel.LineItem2.Quantity = 2;
            viewModel.LineItem2.Price = 50.00m;
            viewModel.CreateLineItem3();
            viewModel.LineItem3 = null;

            // ASSERT
            AssertNotificationsEqual();
        }

        public static readonly IEnumerable<object[]> TestCases_for_ViewModel_with_nested_properties_should_notify = new[]
        {
            new object[] { new TotalLineItemsViewModelManual() },
            new object[] { new TotalLineItemsViewModelChainedWithoutAttributes() },
            new object[] { new TotalLineItemsViewModelChained() },
        };

    }

    public interface ITotalLineItemsViewModel : INotifyPropertyChanged
    {
        ILineItemViewModel LineItem1 { get; set; }
        ILineItemViewModel LineItem2 { get; set; }
        ILineItemViewModel LineItem3 { get; set; }

        void CreateLineItem1();
        void CreateLineItem2();
        void CreateLineItem3();

        int TotalLineItems { get; }
        int TotalItemQuantity { get; }
        decimal TotalCost { get; }
    }

    public interface ILineItemViewModel : INotifyPropertyChanged
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

    public class TotalLineItemsViewModelManual : NotifyPropertyChangedBase, ITotalLineItemsViewModel
    {
        private ILineItemViewModel myLineItem1;

        public ILineItemViewModel LineItem1
        {
            get { return myLineItem1; }
            set
            {
                if (myLineItem1 != null && !ReferenceEquals(myLineItem1, value))
                    LineItem1.PropertyChanged -= OnLineItemPropertyChanged;
                myLineItem1 = value;
                if (myLineItem1 != null)
                    LineItem1.PropertyChanged += OnLineItemPropertyChanged;
                RaisePropertyChanged();
                RaisePropertyChanged(() => TotalLineItems);
                RaisePropertyChanged(() => TotalItemQuantity);
                RaisePropertyChanged(() => TotalCost);
            }
        }

        private ILineItemViewModel myLineItem2;

        public ILineItemViewModel LineItem2
        {
            get { return myLineItem2; }
            set
            {
                if (myLineItem2 != null && !ReferenceEquals(myLineItem2, value))
                    LineItem2.PropertyChanged -= OnLineItemPropertyChanged;
                myLineItem2 = value;
                if (value != null)
                    LineItem2.PropertyChanged += OnLineItemPropertyChanged;
                RaisePropertyChanged();
                RaisePropertyChanged(() => TotalLineItems);
                RaisePropertyChanged(() => TotalItemQuantity);
                RaisePropertyChanged(() => TotalCost);
            }
        }

        private ILineItemViewModel myLineItem3;

        public ILineItemViewModel LineItem3
        {
            get { return myLineItem3; }
            set
            {
                if (myLineItem3 != null && !ReferenceEquals(myLineItem3, value))
                    LineItem3.PropertyChanged -= OnLineItemPropertyChanged;
                myLineItem3 = value;
                if (value != null)
                    LineItem3.PropertyChanged += OnLineItemPropertyChanged;
                RaisePropertyChanged();
                RaisePropertyChanged(() => TotalLineItems);
                RaisePropertyChanged(() => TotalItemQuantity);
                RaisePropertyChanged(() => TotalCost);
            }
        }

        public void CreateLineItem1() => LineItem1 = new LineItemViewModelManual();
        public void CreateLineItem2() => LineItem2 = new LineItemViewModelManual();
        public void CreateLineItem3() => LineItem3 = new LineItemViewModelManual();

        private void OnLineItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Quantity")
            {
                RaisePropertyChanged(() => TotalItemQuantity);
            }
            else if (e.PropertyName == "Cost")
            {
                RaisePropertyChanged(() => TotalCost);
            }
        }

        public int TotalLineItems
        {
            get
            {
                return (LineItem1 != null ? 1 : 0)
                       + (LineItem2 != null ? 1 : 0)
                       + (LineItem3 != null ? 1 : 0);
            }
        }

        public int TotalItemQuantity
        {
            get
            {
                return (LineItem1 != null ? LineItem1.Quantity : 0)
                       + (LineItem2 != null ? LineItem2.Quantity : 0)
                       + (LineItem3 != null ? LineItem3.Quantity : 0);
            }
        }

        public decimal TotalCost
        {
            get
            {
                return (LineItem1 != null ? LineItem1.Cost : 0)
                       + (LineItem2 != null ? LineItem2.Cost : 0)
                       + (LineItem3 != null ? LineItem3.Cost : 0);
            }
        }
    }

    public class LineItemViewModelManual : NotifyPropertyChangedBase, ILineItemViewModel
    {
        private int myQuantity;
        public int Quantity
        {
            get => myQuantity;
            set
            {
                myQuantity = value;
                RaisePropertyChanged();
                RaisePropertyChanged(() => Cost);
            }
        }

        private decimal myPrice;
        public decimal Price
        {
            get => myPrice;
            set
            {
                myPrice = value;
                RaisePropertyChanged();
                RaisePropertyChanged(() => Cost);
            }
        }

        public decimal Cost => Quantity * Price;
    }

    public class TotalLineItemsViewModelChainedWithoutAttributes : NotifyPropertyChangedBase, ITotalLineItemsViewModel
    {
        private ILineItemViewModel myLineItem1;
        public ILineItemViewModel LineItem1
        {
            get { return myLineItem1; }
            set
            {
                myLineItem1 = value;
                InitializeLineItem(myLineItem1 as LineItemViewModelChainedWithoutAttributes);
                RaisePropertyChanged();
            }
        }

        private ILineItemViewModel myLineItem2;
        public ILineItemViewModel LineItem2
        {
            get { return myLineItem2; }
            set
            {
                myLineItem2 = value;
                InitializeLineItem(myLineItem2 as LineItemViewModelChainedWithoutAttributes);
                RaisePropertyChanged();
            }
        }

        private ILineItemViewModel myLineItem3;
        public ILineItemViewModel LineItem3
        {
            get { return myLineItem3; }
            set
            {
                myLineItem3 = value;
                InitializeLineItem(myLineItem3 as LineItemViewModelChainedWithoutAttributes);
                RaisePropertyChanged();
            }
        }

        public void CreateLineItem1() => LineItem1 = new LineItemViewModelChainedWithoutAttributes();
        public void CreateLineItem2() => LineItem2 = new LineItemViewModelChainedWithoutAttributes();
        public void CreateLineItem3() => LineItem3 = new LineItemViewModelChainedWithoutAttributes();

        private void InitializeLineItem(LineItemViewModelChainedWithoutAttributes li)
        {
            System.Diagnostics.Debugger.Break(); // try removing this section
            if (li == null) return;

            // In a traditional MVVM app, this won't be necessary because the databinding will call the necessary getters for us and initialize the chains
            var cost = li.Cost;
        }

        public int TotalLineItems
        {
            get
            {
                myNotificationChainManager.CreateOrGet()
                                          .Configure(cn => cn.On(() => LineItem1)
                                                             .On(() => LineItem2)
                                                             .On(() => LineItem3)
                                                             .Finish());

                return (LineItem1 != null ? 1 : 0)
                       + (LineItem2 != null ? 1 : 0)
                       + (LineItem3 != null ? 1 : 0);
            }
        }

        public int TotalItemQuantity
        {
            get
            {
                myNotificationChainManager.CreateOrGet()
                                          .Configure(cn => cn.On(() => LineItem1, li => li.Quantity)
                                                             .On(() => LineItem2, li => li.Quantity)
                                                             .On(() => LineItem3, li => li.Quantity)
                                                             .Finish());

                return (LineItem1 != null ? LineItem1.Quantity : 0)
                       + (LineItem2 != null ? LineItem2.Quantity : 0)
                       + (LineItem3 != null ? LineItem3.Quantity : 0);
            }
        }

        public decimal TotalCost
        {
            get
            {
                myNotificationChainManager.CreateOrGet()
                                          .Configure(cn => cn.On(() => LineItem1, li => li.Cost)
                                                             .On(() => LineItem2, li => li.Cost)
                                                             .On(() => LineItem3, li => li.Cost)
                                                             .Finish());

                return (LineItem1 != null ? LineItem1.Cost : 0)
                       + (LineItem2 != null ? LineItem2.Cost : 0)
                       + (LineItem3 != null ? LineItem3.Cost : 0);
            }
        }
    }

    public class LineItemViewModelChainedWithoutAttributes : NotifyPropertyChangedBase, ILineItemViewModel
    {
        private int myQuantity;
        public int Quantity
        {
            get { return myQuantity; }
            set
            {
                myQuantity = value;
                RaisePropertyChanged();
            }
        }

        private decimal myPrice;
        public decimal Price
        {
            get { return myPrice; }
            set
            {
                myPrice = value;
                RaisePropertyChanged();
            }
        }

        public virtual decimal Cost
        {
            get
            {
                myNotificationChainManager.CreateOrGet()
                                          .Configure(cn => cn.On(() => Quantity)
                                                             .On(() => Price)
                                                             .Finish());

                return Quantity * Price;
            }
        }
    }

    public class TotalLineItemsViewModelChained : NotifyPropertyChangedBase, ITotalLineItemsViewModel
    {
        private ILineItemViewModel myLineItem1;
        public ILineItemViewModel LineItem1
        {
            get { return myLineItem1; }
            set
            {
                myLineItem1 = value;
                RaisePropertyChanged();
            }
        }

        private ILineItemViewModel myLineItem2;
        public ILineItemViewModel LineItem2
        {
            get { return myLineItem2; }
            set
            {
                myLineItem2 = value;
                RaisePropertyChanged();
            }
        }

        private ILineItemViewModel myLineItem3;
        public ILineItemViewModel LineItem3
        {
            get { return myLineItem3; }
            set
            {
                myLineItem3 = value;
                RaisePropertyChanged();
            }
        }

        public void CreateLineItem1() => LineItem1 = new LineItemViewModelChained();
        public void CreateLineItem2() => LineItem2 = new LineItemViewModelChained();
        public void CreateLineItem3() => LineItem3 = new LineItemViewModelChained();

        [NotificationChainProperty]
        public int TotalLineItems
        {
            get
            {
                myNotificationChainManager.CreateOrGet()
                                          .Configure(cn => cn.On(() => LineItem1)
                                                             .On(() => LineItem2)
                                                             .On(() => LineItem3)
                                                             .Finish());

                return (LineItem1 != null ? 1 : 0)
                       + (LineItem2 != null ? 1 : 0)
                       + (LineItem3 != null ? 1 : 0);
            }
        }

        [NotificationChainProperty]
        public int TotalItemQuantity
        {
            get
            {
                myNotificationChainManager.CreateOrGet()
                                          .Configure(cn => cn.On(() => LineItem1, li => li.Quantity)
                                                             .On(() => LineItem2, li => li.Quantity)
                                                             .On(() => LineItem3, li => li.Quantity)
                                                             .Finish());

                return (LineItem1 != null ? LineItem1.Quantity : 0)
                       + (LineItem2 != null ? LineItem2.Quantity : 0)
                       + (LineItem3 != null ? LineItem3.Quantity : 0);
            }
        }

        [NotificationChainProperty]
        public decimal TotalCost
        {
            get
            {
                myNotificationChainManager.CreateOrGet()
                                          .Configure(cn => cn.On(() => LineItem1, li => li.Cost)
                                                             .On(() => LineItem2, li => li.Cost)
                                                             .On(() => LineItem3, li => li.Cost)
                                                             .Finish());

                return (LineItem1 != null ? LineItem1.Cost : 0)
                       + (LineItem2 != null ? LineItem2.Cost : 0)
                       + (LineItem3 != null ? LineItem3.Cost : 0);
            }
        }
    }

    public class LineItemViewModelChained : NotifyPropertyChangedBase, ILineItemViewModel
    {
        private int myQuantity;
        public int Quantity
        {
            get { return myQuantity; }
            set
            {
                myQuantity = value;
                RaisePropertyChanged();
            }
        }

        private decimal myPrice;
        public decimal Price
        {
            get { return myPrice; }
            set
            {
                myPrice = value;
                RaisePropertyChanged();
            }
        }

        [NotificationChainProperty]
        public decimal Cost
        {
            get
            {
                myNotificationChainManager.CreateOrGet()
                                          .Configure(cn => cn.On(() => Quantity)
                                                             .On(() => Price)
                                                             .Finish());

                return Quantity * Price;
            }
        }
    }
}
