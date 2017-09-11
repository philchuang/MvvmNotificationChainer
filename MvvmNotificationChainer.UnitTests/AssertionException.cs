using System;

namespace MvvmNotificationChainer.UnitTests
{
    public class AssertionException : Exception
    {
        public AssertionException(string message) : base(message)
        {
        }
    }
}
