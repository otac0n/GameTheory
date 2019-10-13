// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.LoveLetter.Tests
{
    using System.Threading.Tasks;
    using GameTheory.Players;
    using NUnit.Framework;

    [TestFixture]
    public class GameStateTests
    {
        public const int Samples = 1000;

        [Test]
        public async Task ctor_Always_ReturnsAGameStateThatCanBePlayedToTheEnd()
        {
            for (var i = 0; i < Samples; i++)
            {
                var state = new GameState();
                state = (GameState)await GameUtilities.PlayGame(state, p => new RandomPlayer<Move>(p));
                Assert.That(state.GetWinners(), Is.Not.Empty);
            }
        }
    }
}
