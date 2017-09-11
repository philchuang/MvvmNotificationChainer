using System.Collections.Generic;
using System.Linq;
using Com.PhilChuang.Utils;
using Xunit;

namespace MvvmNotificationChainer.UnitTests
{
    public abstract class TestBase
    {
        public static void AssertSequenceEquals<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            if (expected == null && actual == null) return;
            if (expected == null)
                throw new AssertionException("Expected null, got non-null");
            if (actual == null)
                throw new AssertionException("Expected non-null, got null");

            var expectedCount = 0;
            var actualCount = 0;
            using (var expectedEnumerator = expected.GetEnumerator())
            using (var actualEnumerator = actual.GetEnumerator())
            {
                do
                {
                    var expectedMoveNext = expectedEnumerator.MoveNext();
                    var actualMoveNext = actualEnumerator.MoveNext();

                    if (expectedMoveNext) expectedCount++;
                    if (actualMoveNext) actualCount++;

                    if (expectedMoveNext != actualMoveNext)
                    {
                        if (expectedMoveNext)
                            throw new AssertionException($"Sequence count mismatch, expected at least {expectedCount} items, got {actualCount} items");
                        if (actualMoveNext)
                            throw new AssertionException($"Sequence count mismatch, expected {expectedCount} items, got {actualCount} items");
                    }

                    if (!expectedMoveNext) return;

                    Assert.Equal(expectedEnumerator.Current, actualEnumerator.Current);

                } while (true);
            }
        }

        public static void AssertListEquals<T>(IList<T> expected, IList<T> actual)
        {
            if (expected == null && actual == null) return;
            if (expected == null)
                throw new AssertionException("Expected null, got non-null");
            if (actual == null)
                throw new AssertionException("Expected non-null, got null");

            if (expected.Count != actual.Count)
                throw new AssertionException("Expected\n[{0}], got \n[{1}]"
                    .FormatWith(string.Join(", ", expected.Select(i => i.ToString())),
                        string.Join(", ", actual.Select(i => i.ToString()))));

            for (var idx = 0; idx < expected.Count; idx++)
            {
                if (!expected[idx].Equals(actual[idx]))
                    throw new AssertionException("Expected\n[{0}], got \n[{1}]"
                        .FormatWith(string.Join(", ", expected.Select(i => i.ToString())),
                            string.Join(", ", actual.Select(i => i.ToString()))));
            }
        }
    }
}
