using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NBehave.Spec.NUnit;
using NUnit.Framework;

namespace MvvmNotificationChainer.UnitTests
{
    [TestFixture]
    public abstract class MvvmNotificationChainer_UnitTests_Base : SpecBase
    {
        protected Exception m_BecauseOfException;
        protected bool m_IsBecauseOfExceptionExpected = false;
        protected Exception m_ExpectedBecauseOfException;

        [Test]
        public virtual void AssertExceptionMatches()
        {
            if (!m_IsBecauseOfExceptionExpected)
            {
                Assert.IsNull(m_BecauseOfException, "Expected m_BecauseOfException to be null");
                return;
            }

            Assert.IsNotNull(m_BecauseOfException, "Expected m_BecauseOfException to not be null");
            Assert.IsNotNull(m_ExpectedBecauseOfException, "m_ExpectedBecauseOfException was not provided");
            Assert.AreEqual(m_ExpectedBecauseOfException.GetType(), m_BecauseOfException.GetType(),
                             "Expected m_BecauseOfException to be {0} \"{1}\", got {2} \"{3}\"",
                             m_ExpectedBecauseOfException.GetType().Name,
                             m_ExpectedBecauseOfException.Message,
                             m_BecauseOfException.GetType().Name,
                             m_BecauseOfException.Message);
            Assert.AreEqual(m_ExpectedBecauseOfException.Message, m_BecauseOfException.Message,
                             "Expected m_BecauseOfException to be {0}, got {1}", m_ExpectedBecauseOfException.Message, m_BecauseOfException.Message);
        }
    }
}
