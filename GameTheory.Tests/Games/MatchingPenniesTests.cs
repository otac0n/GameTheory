// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Tests.Games
{
    using GameTheory.Games.MatchingPennies;
    using NUnit.Framework;

    [TestFixture]
    public class MatchingPenniesTests
    {
        [TestCase(true)]
        [TestCase(false)]
        public void GetWinners_WhenPenniesMatch_ReturnsFirstPlayer(bool heads)
        {
            var state = new GameState();
            state = (GameState)state.PlayMove(state.Players[0], m => m.Heads == heads);
            state = (GameState)state.PlayMove(state.Players[1], m => m.Heads == heads);
            Assert.That(state.GetWinners(), Is.EqualTo(new[] { state.Players[0] }));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GetWinners_WhenPenniesDontMatch_ReturnsSecondPlayer(bool firstPlayerHeads)
        {
            var state = new GameState();
            state = (GameState)state.PlayMove(state.Players[0], m => m.Heads == firstPlayerHeads);
            state = (GameState)state.PlayMove(state.Players[1], m => m.Heads != firstPlayerHeads);
            Assert.That(state.GetWinners(), Is.EqualTo(new[] { state.Players[1] }));
        }
    }
}
