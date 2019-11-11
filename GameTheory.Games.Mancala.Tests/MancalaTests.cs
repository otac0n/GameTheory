// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Mancala.Tests
{
    using System;
    using System.Linq;
    using GameTheory.Players;
    using NUnit.Framework;

    [TestFixture]
    public class MancalaTests
    {
        [TestCase("(5),(7),(4),(7),(3),(12),(0),(7),(1),(7),(2),(7),(3),(7),(4),(7),(5)")]
        [TestCase("(1),(9),(12),(0),(8),(3),(12),(9),(2),(7),(12),(10),(0),(11),(1),(5),(2),(12),(0),(9),(5),(2),(10),(4),(12),(11),(12),(9),(0),(7),(2),(8),(12),(10),(12),(11),(12),(9),(1),(10),(2),(11),(5),(3),(7),(5),(4),(8),(5)")]
        public void GetAvailableMoves_WhenOnePlayersSideIsEmpty_YieldsNone(string moves)
        {
            var state = new GameState();
            foreach (var move in moves.Split(','))
            {
                state = state.MakeMove(state.GetAvailableMoves().Where(m => m.ToString().Contains(move)).Single());
            }

            Assert.That(state.GetAvailableMoves(), Is.Empty);
        }

        [Test]
        public void GetWinners_AfterAGameHasBeenPlayed_ReturnsThePlayersWithTheHighestScore()
        {
            var endState = GameUtilities.PlayGame(
                new GameState(),
                p => new RandomPlayer<GameState, Move>(p),
                (prevState, move, state) => Console.WriteLine("{0}: {1}", state.GetPlayerName<GameState, Move>(move.PlayerToken), move)).Result;

            var highestScore = endState.Players.Max(p => endState.GetScore(p));
            var winners = endState.GetWinners();

            Assert.That(winners, Is.EqualTo(endState.Players.Where(p => endState.GetScore(p) == highestScore)));
        }
    }
}
