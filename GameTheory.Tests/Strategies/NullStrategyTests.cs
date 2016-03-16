// Copyright © 2016 John Gietzen.  All Rights Reserved.
// This source is subject to the MIT license.
// Please see license.md for more information.

namespace GameTheory.Tests.Strategies
{
    using System.Threading;
    using System.Threading.Tasks;
    using GameTheory.Strategies;
    using NUnit.Framework;

    [TestFixture]
    public class NullStrategyTests
    {
        [Test]
        public async Task GetMove_Always_ReturnsNoMove()
        {
            var gameState = new StubGameState();
            using (var strategy = new NullStrategy<StubGameState.Move>())
            {
                var maybeMove = await strategy.ChooseMove(gameState, CancellationToken.None);
                Assert.That(maybeMove.HasValue, Is.False);
            }
        }
    }
}
