// Copyright © 2016 John Gietzen.  All Rights Reserved.
// This source is subject to the MIT license.
// Please see license.md for more information.

namespace GameTheory.Tests
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
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

            ImmutableList<int> dealt;
            list.Deal(count, out dealt);

            Assert.That(dealt, Is.EquivalentTo(list));
        }

        [Theory]
        public void Deal_WhenAskedForAllItems_ReturnsAnEmptyList(int count)
        {
            var list = Enumerable.Range(0, count).ToImmutableList();

            ImmutableList<int> dealt;
            list = list.Deal(count, out dealt);

            Assert.That(list, Is.Empty);
        }

        [Theory]
        public void Deal_WhenAskedForFewerCardsThanTheDeckContains_OutputsEmptyList(int count, int deal)
        {
            Assume.That(deal, Is.GreaterThan(0));
            Assume.That(deal, Is.LessThan(count));

            var list = Enumerable.Range(0, count).ToImmutableList();

            ImmutableList<int> dealt;
            list.Deal(deal, out dealt);

            Assert.That(dealt.Count, Is.EqualTo(deal));
        }

        [Theory]
        public void Deal_WhenAskedForFewerCardsThanTheDeckContains_ReturnsAListContainingTheRemainder(int count, int deal)
        {
            Assume.That(deal, Is.GreaterThan(0));
            Assume.That(deal, Is.LessThan(count));

            var list = Enumerable.Range(0, count).ToImmutableList();

            ImmutableList<int> dealt;
            list = list.Deal(deal, out dealt);

            Assert.That(list.Count, Is.EqualTo(count - deal));
        }

        [Theory]
        public void Deal_WhenAskedForZeroCards_OutputsEmptyList(int count)
        {
            var list = Enumerable.Range(0, count).ToImmutableList();

            ImmutableList<int> dealt;
            list.Deal(0, out dealt);

            Assert.That(dealt, Is.Empty);
        }

        [Theory]
        public void Deal_WhenAskedForZeroCards_ReturnsOriginalList(int count)
        {
            var list = Enumerable.Range(0, count).ToImmutableList();

            ImmutableList<int> dealt;
            var newList = list.Deal(0, out dealt);

            Assert.That(newList, Is.SameAs(list));
        }

        [Theory]
        public void Deal_WithInsufficientCards_OutputsAllExisting(int count, int extra)
        {
            var list = Enumerable.Range(0, count).ToImmutableList();

            ImmutableList<int> dealt;
            list.Deal(count + extra, out dealt);

            Assert.That(dealt, Is.EquivalentTo(list));
        }

        [Theory]
        public void Deal_WithInsufficientCards_ReturnsAnEmptyList(int count, int extra)
        {
            var list = Enumerable.Range(0, count).ToImmutableList();

            ImmutableList<int> dealt;
            list = list.Deal(count + extra, out dealt);

            Assert.That(list, Is.Empty);
        }

        [Test]
        public void Instance_WhenCalledFromDifferentThreads_ReturnsDifferentInstances()
        {
            System.Random a = null, b = null;
            var threads = new List<Thread>
            {
                new Thread(() => { a = Random.Instance; }),
                new Thread(() => { b = Random.Instance; }),
            };

            threads.ForEach(t => t.Start());
            threads.ForEach(t => t.Join());

            Assume.That(new[] { a, b }, Is.All.Not.Null);
            Assert.That(a, Is.Not.SameAs(b));
        }

        [Test]
        public void Instance_WhenCreatedSimultaneouslyWithAnotherThread_DoesNotUseTheSameSeed()
        {
            List<int> a = null, b = null;
            var flag = new ManualResetEventSlim();
            var waiting = 0;
            var threads = new List<Thread>
            {
                new Thread(() =>
                {
                    Interlocked.Increment(ref waiting);
                    flag.Wait();
                    var rand = Random.Instance;
                    a = Enumerable.Range(0, 10).Select(i => rand.Next()).ToList();
                }),
                new Thread(() =>
                {
                    Interlocked.Increment(ref waiting);
                    flag.Wait();
                    var rand = Random.Instance;
                    b = Enumerable.Range(0, 10).Select(i => rand.Next()).ToList();
                }),
            };
            threads.ForEach(t => t.Start());
            while (waiting < 2)
            {
                Thread.Sleep(0);
            }

            flag.Set();
            threads.ForEach(t => t.Join());

            Assume.That(new[] { a, b }, Is.All.Not.Null);
            Assert.That(a, Is.Not.EquivalentTo(b));
        }
    }
}
