// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Tests.Games
{
    using System;
    using System.Linq;
    using GameTheory.Games.Mancala;
    using GameTheory.Players;
    using NUnit.Framework;

    [TestFixture]
    public class MancalaTests
    {
        [Test]
        public void GetWinners_AfterAGameHasBeenPlayed_ReturnsThePlayersWithTheHighestScore()
        {
            var endState = (GameState)GameUtils.PlayGame(
                new GameState(),
                p => new RandomPlayer<Move>(p),
                (state, move) => Console.WriteLine("{0}: {1}", state.PlayerName(move.PlayerToken), move)).Result;

            var highestScore = endState.Players.Max(p => endState.GetScore(p));
            var winners = endState.GetWinners();

            Assert.That(winners, Is.EqualTo(endState.Players.Where(p => endState.GetScore(p) == highestScore)));
        }
    }
}
