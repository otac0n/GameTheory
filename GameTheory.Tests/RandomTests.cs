// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Tests
{
    using System.Collections.Immutable;
    using System.Linq;
    using NUnit.Framework;

    [TestFixture]
    public class RandomTests
    {
        [Datapoints]
        private int[] datapoints = new[] { 0, 1, 2, 3, 5, 8, 13, 21, 25, 50, 100, 1000 };

        [Theory]
        public void Deal_WhenAskedForAllItems_OutputsAllOfTheItems(int count)
        {
            var list = Enumerable.Range(0, count).ToImmutableList();

            list.Deal(count, out var dealt);

            Assert.That(dealt, Is.EquivalentTo(list));
        }

        [Theory]
        public void Deal_WhenAskedForAllItems_ReturnsAnEmptyList(int count)
        {
            var list = Enumerable.Range(0, count).ToImmutableList();

            list = list.Deal(count, out var dealt);

            Assert.That(list, Is.Empty);
        }

        [Theory]
        public void Deal_WhenAskedForFewerCardsThanTheDeckContains_OutputsEmptyList(int count, int deal)
        {
            Assume.That(deal, Is.GreaterThan(0));
            Assume.That(deal, Is.LessThan(count));

            var list = Enumerable.Range(0, count).ToImmutableList();

            list.Deal(deal, out var dealt);

            Assert.That(dealt.Count, Is.EqualTo(deal));
        }

        [Theory]
        public void Deal_WhenAskedForFewerCardsThanTheDeckContains_ReturnsAListContainingTheRemainder(int count, int deal)
        {
            Assume.That(deal, Is.GreaterThan(0));
            Assume.That(deal, Is.LessThan(count));

            var list = Enumerable.Range(0, count).ToImmutableList();

            list = list.Deal(deal, out var dealt);

            Assert.That(list.Count, Is.EqualTo(count - deal));
        }

        [Theory]
        public void Deal_WhenAskedForZeroCards_OutputsEmptyList(int count)
        {
            var list = Enumerable.Range(0, count).ToImmutableList();

            list.Deal(0, out var dealt);

            Assert.That(dealt, Is.Empty);
        }

        [Theory]
        public void Deal_WhenAskedForZeroCards_ReturnsOriginalList(int count)
        {
            var list = Enumerable.Range(0, count).ToImmutableList();

            var newList = list.Deal(0, out var dealt);

            Assert.That(newList, Is.SameAs(list));
        }

        [Theory]
        public void Deal_WithInsufficientCards_OutputsAllExisting(int count, int extra)
        {
            var list = Enumerable.Range(0, count).ToImmutableList();

            list.Deal(count + extra, out var dealt);

            Assert.That(dealt, Is.EquivalentTo(list));
        }

        [Theory]
        public void Deal_WithInsufficientCards_ReturnsAnEmptyList(int count, int extra)
        {
            var list = Enumerable.Range(0, count).ToImmutableList();

            list = list.Deal(count + extra, out var dealt);

            Assert.That(list, Is.Empty);
        }
    }
}
