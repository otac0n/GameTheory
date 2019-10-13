// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Tests
{
    using System;
    using System.Linq;
    using NUnit.Framework;

    [TestFixture]
    public class SizeTests
    {
        [Datapoints]
        public static readonly int[] Dimensions = new[] { 1, 2, 3, 4, 8 };

        [Theory]
        public void Count_Always_ReturnsTheSizeOfTheEnumerableCollection(int width, int height)
        {
            var size = new Size(width, height);
            var points = size.ToList();

            Assert.That(size.Count, Is.EqualTo(points.Count));
        }

        [Theory]
        public void Indexer_WhenGivenAnIndexOutsideTheSize_ThrowsArgumentOutOfRangeException(int width, int height)
        {
            var size = new Size(width, height);

            Assert.That(() => size[-1], Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => size[width * height], Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Theory]
        public void Indexer_WhenGivenAnIndexWithinTheSize_ReturnsTheSpecifiedPoint(int width, int height)
        {
            var size = new Size(width, height);

            var points = Enumerable.Range(0, width * height).Select(i => size[i]);

            Assert.That(points, Is.EquivalentTo(size.ToList()));
        }

        [Theory]
        public void IndexOf_WhenGivenAPointContainedInTheSize_ReturnsTheIndexOfThePoint(int width, int height)
        {
            var size = new Size(width, height);
            var points = size.ToList();

            var indexes = points.Select(p => size.IndexOf(p));

            Assert.That(indexes, Is.EquivalentTo(Enumerable.Range(0, size.Count)));
        }

        [Theory]
        public void IndexOf_WhenGivenAPointNotContainedInTheSize_ReturnsNegativeOne(int width, int height)
        {
            var size = new Size(width, height);
            var points = new[] { new Point(-1, -1), new Point(width, height), new Point(width, 0), new Point(0, height) };

            var indexes = points.Select(p => size.IndexOf(p));

            Assert.That(indexes, Is.All.EqualTo(-1));
        }
    }
}
