using System.Collections.Generic;
using Xunit;

// ReSharper disable InconsistentNaming
namespace MvvmNotificationChainer.UnitTests
{
    public class SimpleNotificationTests : NotificationTestBase
    {
        [Theory]
        [MemberData(nameof(TestCases_for_SimpleViewModel_should_notify))]
        public void SimpleViewModel_should_notify(ILineItemViewModel viewModel)
        {
            viewModel.PropertyChanged += (_, e) => ActualNotifications.Add(e.PropertyName);

            ExpectedNotifications.AddRange(new[]
                                           {
                                               //viewModel.Quantity = 1;
                                               "Quantity",
                                               "Cost",
                                               //viewModel.Price = 99.99m;
                                               "Price",
                                               "Cost"
                                           });

            viewModel.Quantity = 1;
            viewModel.Price = 99.99m;

            AssertNotificationsEqual();
        }

        public static readonly IEnumerable<object[]> TestCases_for_SimpleViewModel_should_notify = new[]
        {
            new object[] { new LineItemViewModelManual() },
            new object[] { new LineItemViewModelChainedWithoutAttributes() },
            new object[] { new LineItemViewModelChained() },
        };
    }
}
