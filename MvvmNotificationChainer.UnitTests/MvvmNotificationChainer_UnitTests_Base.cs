using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Com.PhilChuang.Utils;
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

        public static void AssertSequenceEquals<T> (IEnumerable<T> expected, IEnumerable<T> actual)
        {
            if (expected == null && actual == null) return;
            if (expected == null)
                throw new AssertionException("Expected null, got non-null");
            if (actual == null)
                throw new AssertionException("Expected non-null, got null");

            var expectedCount = 0;
            var actualCount = 0;
            var expectedEnumerator = expected.GetEnumerator ();
            var actualEnumerator = actual.GetEnumerator ();
            do
            {
                var expectedMoveNext = expectedEnumerator.MoveNext ();
                var actualMoveNext = actualEnumerator.MoveNext ();

                if (expectedMoveNext) expectedCount++;
                if (actualMoveNext) actualCount++;

                if (expectedMoveNext != actualMoveNext)
                {
                    if (expectedMoveNext)
                        throw new AssertionException ("Sequence count mismatch, expected at least {0} items, got {1} items".FormatWith (expectedCount, actualCount));
                    if (actualMoveNext)
                        throw new AssertionException ("Sequence count mismatch, expected {0} items, got {1} items".FormatWith (expectedCount, actualCount));
                }

                if (!expectedMoveNext && !actualMoveNext) return;

                Assert.AreEqual (expectedEnumerator.Current, actualEnumerator.Current);

            } while (true);
        }

        public static void AssertListEquals<T> (IList<T> expected, IList<T> actual)
        {
            if (expected == null && actual == null) return;
            if (expected == null)
                throw new AssertionException("Expected null, got non-null");
            if (actual == null)
                throw new AssertionException("Expected non-null, got null");

            if (expected.Count != actual.Count)
                throw new AssertionException ("Expected\n[{0}], got \n[{1}]"
                                                  .FormatWith (expected.Select (i => i.ToString ()).Aggregate ((s1, s2) => s1 + ", " + s2),
                                                               actual.Select (i => i.ToString ()).Aggregate ((s1, s2) => s1 + ", " + s2)));

            for (var idx = 0; idx < expected.Count; idx++)
            {
                if (!expected[idx].Equals (actual[idx]))
                    throw new AssertionException ("Expected\n[{0}], got \n[{1}]"
                                                      .FormatWith (expected.Select (i => i.ToString ()).Aggregate ((s1, s2) => s1 + ", " + s2),
                                                                   actual.Select (i => i.ToString ()).Aggregate ((s1, s2) => s1 + ", " + s2)));
            }
        }
    }
}
