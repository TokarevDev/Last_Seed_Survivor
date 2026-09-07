using System;
using System.Collections.Generic;
using LastSeed.Core.Collections;
using NUnit.Framework;

namespace LastSeed.Tests
{
    public sealed class SortedSearchTests
    {
        [TestCase(-1, -1)]
        [TestCase(0, 0)]
        [TestCase(4, 0)]
        [TestCase(5, 2)]
        [TestCase(9, 2)]
        [TestCase(10, 3)]
        [TestCase(11, 3)]
        public void FindLastIndexAtMost_ReturnsUpperBoundAcrossDuplicates(
            int value,
            int expectedIndex)
        {
            int[] sortedItems = { 0, 5, 5, 10 };

            int result = SortedSearch.FindLastIndexAtMost(sortedItems, value);

            Assert.That(result, Is.EqualTo(expectedIndex));
        }

        [Test]
        public void FindLastIndexAtMost_EmptyCollection_ReturnsMissingIndex()
        {
            int result = SortedSearch.FindLastIndexAtMost(Array.Empty<int>(), 5);

            Assert.That(result, Is.EqualTo(-1));
        }

        [Test]
        public void FindLastIndexAtMost_UsesProvidedComparer()
        {
            string[] sortedItems = { "A", "b", "C" };

            int result = SortedSearch.FindLastIndexAtMost(
                sortedItems,
                "B",
                StringComparer.OrdinalIgnoreCase);

            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public void FindLastIndexAtMost_NullCollection_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                SortedSearch.FindLastIndexAtMost<int>(null, 0));
        }
    }
}
