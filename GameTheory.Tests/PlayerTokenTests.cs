// Copyright © 2016 John Gietzen.  All Rights Reserved.
// This source is subject to the MIT license.
// Please see license.md for more information.

namespace GameTheory.Tests
{
    using NUnit.Framework;

    [TestFixture]
    public class PlayerTokenTests
    {
        [Test]
        public void Equals_WhenGivenADifferentInstance_ReturnsFalse()
        {
            var a = new PlayerToken();
            var b = new PlayerToken();

            var result = a.Equals(b);

            Assert.That(result, Is.False);
        }

        [Test]
        public void Equals_WhenGivenTheSameInstance_ReturnsTrue()
        {
            var a = new PlayerToken();

            var result = a.Equals(a);

            Assert.That(result, Is.True);
        }
    }
}
