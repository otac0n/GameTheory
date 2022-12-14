// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Tests.Games
{
    using GameTheory.Games.NormalFormGame;
    using GameTheory.Games.NormalFormGame.MatchingPennies;
    using GameTheory.Testing;
    using NUnit.Framework;

    [TestFixture]
    public class MatchingPenniesTests
    {
        [TestCase(GameState.Heads)]
        [TestCase(GameState.Tails)]
        public void GetWinners_WhenPenniesDontMatch_ReturnsSecondPlayer(string firstPlayerMove)
        {
            var state = new GameState();
            state = state.PlayMove<GameState, Move<string>>(state.Players[0], m => m.Kind == firstPlayerMove);
            state = state.PlayMove<GameState, Move<string>>(state.Players[1], m => m.Kind != firstPlayerMove);
            Assert.That(state.GetWinners(), Is.EqualTo(new[] { state.Players[1] }));
        }

        [TestCase(GameState.Heads)]
        [TestCase(GameState.Tails)]
        public void GetWinners_WhenPenniesMatch_ReturnsFirstPlayer(string move)
        {
            var state = new GameState();
            state = state.PlayMove<GameState, Move<string>>(state.Players[0], m => m.Kind == move);
            state = state.PlayMove<GameState, Move<string>>(state.Players[1], m => m.Kind == move);
            Assert.That(state.GetWinners(), Is.EqualTo(new[] { state.Players[0] }));
        }
    }
}
