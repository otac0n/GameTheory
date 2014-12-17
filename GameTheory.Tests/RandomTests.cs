// -----------------------------------------------------------------------
// <copyright file="RandomTests.cs" company="(none)">
//   Copyright © 2014 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using NUnit.Framework;

    [TestFixture]
    public class RandomTests
    {
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
                new Thread(() => { Interlocked.Increment(ref waiting); flag.Wait(); var rand = Random.Instance; a = Enumerable.Range(0, 10).Select(i => rand.Next()).ToList(); }),
                new Thread(() => { Interlocked.Increment(ref waiting); flag.Wait(); var rand = Random.Instance; b = Enumerable.Range(0, 10).Select(i => rand.Next()).ToList(); }),
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
