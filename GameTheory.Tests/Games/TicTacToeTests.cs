// Copyright © 2016 John Gietzen.  All Rights Reserved.
// This source is subject to the MIT license.
// Please see license.md for more information.

namespace GameTheory.Tests.Games
{
    using System.Linq;
    using GameTheory.Games;
    using NUnit.Framework;

    [TestFixture]
    public class TicTacToeTests
    {
        private string[] draws =
        {
            "0,0; 1,1; 2,2; 2,1; 0,1; 0,2; 2,0; 1,0; 1,2",
        };

        private string[] player1Wins =
        {
            "0,0; 1,1; 0,1; 2,2; 0,2",
            "0,0; 1,1; 2,2; 0,2; 2,0; 1,0; 2,1",
        };

        private string[] player2Wins =
        {
            "0,0; 1,1; 2,2; 1,0; 2,0; 1,2",
        };

        private string[] unfinished =
        {
            "0,0",
            "0,1",
            "1,1",
            "0,2",
            "2,2",
            "0,0; 1,1; 2,2",
        };

        [TestCaseSource("unfinished")]
        public void GetAvailableMoves_WhenTheGameHasNotCompleted_ReturnsANonEmptyListOfMoves(string moveList)
        {
            var state = ApplyMoves(new TicTacToe(), moveList);

            var moves = state.GetAvailableMoves();

            Assert.That(moves, Is.Not.Empty);
        }

        [TestCaseSource("draws")]
        public void GetAvailableMoves_WhenTheGameResultsInADraw_ReturnsAnEmptyList(string moveList)
        {
            var state = ApplyMoves(new TicTacToe(), moveList);

            var moves = state.GetAvailableMoves();

            Assert.That(moves, Is.Empty);
        }

        [TestCaseSource("player1Wins")]
        public void GetAvailableMoves_WhenTheGameResultsInAWinForPlayer1_ReturnsAnEmptyList(string moveList)
        {
            var state = ApplyMoves(new TicTacToe(), moveList);

            var moves = state.GetAvailableMoves();

            Assert.That(moves, Is.Empty);
        }

        [TestCaseSource("player2Wins")]
        public void GetAvailableMoves_WhenTheGameResultsInAWinForPlayer2_ReturnsAnEmptyList(string moveList)
        {
            var state = ApplyMoves(new TicTacToe(), moveList);

            var moves = state.GetAvailableMoves();

            Assert.That(moves, Is.Empty);
        }

        [Test]
        public void GetAvailableMoves_WhenTheGameStateIsNew_ReturnsMovesForTheFirstPlayer()
        {
            var state = new TicTacToe();

            var moves = state.GetAvailableMoves();

            Assert.That(moves, Is.All.Property("Player").EqualTo(state.Players[0]));
        }

        [Test]
        public void GetAvailableMoves_WhenTheGameStateIsNew_ReturnsMovesWithinRange()
        {
            var state = new TicTacToe();

            var moves = from m in state.GetAvailableMoves()
                        where m.X < 0 || m.X > 2 || m.Y < 0 || m.Y > 2
                        select m;

            Assert.That(moves, Is.Empty);
        }

        [Test]
        public void GetAvailableMoves_WhenTheGameStateIsNew_ReturnsNineUniqueMoves()
        {
            var state = new TicTacToe();

            var moves = from m in state.GetAvailableMoves()
                        group m by new { m.X, m.Y } into g
                        select g.Key;

            Assert.That(moves.Count(), Is.EqualTo(9));
        }

        [TestCaseSource("unfinished")]
        public void GetWinners_WhenTheGameHasNotCompleted_ReturnsAnEmptyList(string moveList)
        {
            var state = ApplyMoves(new TicTacToe(), moveList);

            var winners = state.GetWinners();

            Assert.That(winners, Is.Empty);
        }

        [TestCaseSource("draws")]
        public void GetWinners_WhenTheGameResultsInADraw_ReturnsAnEmptyList(string moveList)
        {
            var state = ApplyMoves(new TicTacToe(), moveList);

            var winners = state.GetWinners();

            Assert.That(winners, Is.Empty);
        }

        [TestCaseSource("player1Wins")]
        public void GetWinners_WhenTheGameResultsInAWinForPlayer1_ReturnsAListContainingOnlyPlayer1(string moveList)
        {
            var state = ApplyMoves(new TicTacToe(), moveList);

            var winner = state.GetWinners().SingleOrDefault();

            var player1 = state.Players[0];
            Assert.That(winner, Is.EqualTo(player1));
        }

        [TestCaseSource("player2Wins")]
        public void GetWinners_WhenTheGameResultsInAWinForPlayer2_ReturnsAListContainingOnlyPlayer2(string moveList)
        {
            var state = ApplyMoves(new TicTacToe(), moveList);

            var winner = state.GetWinners().SingleOrDefault();

            var player2 = state.Players[1];
            Assert.That(winner, Is.EqualTo(player2));
        }

        private static T ApplyMoves<T>(T state, string moveList)
            where T : IGameState<TicTacToe.Move>
        {
            var moves = from move in moveList.Split(';')
                        let parts = move.Split(',')
                        let x = int.Parse(parts[0].Trim())
                        let y = int.Parse(parts[1].Trim())
                        select new { x, y };

            return (T)moves.Aggregate((IGameState<TicTacToe.Move>)state, (s, m) => s.MakeMove(s.GetAvailableMoves().Single(a => a.X == m.x && a.Y == m.y)));
        }
    }
}
