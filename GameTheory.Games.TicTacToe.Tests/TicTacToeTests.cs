// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.TicTacToe.Tests
{
    using System.Linq;
    using NUnit.Framework;

    [TestFixture]
    public class TicTacToeTests
    {
        private static string[] draws =
        {
            "0,0; 1,1; 2,2; 2,1; 0,1; 0,2; 2,0; 1,0; 1,2",
        };

        private static string[] player1Wins =
        {
            "0,0; 1,1; 0,1; 2,2; 0,2",
            "0,0; 1,1; 2,2; 0,2; 2,0; 1,0; 2,1",
        };

        private static string[] player2Wins =
        {
            "0,0; 1,1; 2,2; 1,0; 2,0; 1,2",
        };

        private static string[] unfinished =
        {
            "0,0",
            "0,1",
            "1,1",
            "0,2",
            "2,2",
            "0,0; 1,1; 2,2",
        };

        [TestCaseSource(nameof(draws))]
        [TestCaseSource(nameof(player1Wins))]
        [TestCaseSource(nameof(player2Wins))]
        public void GetAvailableMoves_WhenTheGameEnds_ReturnsAnEmptyList(string moveList)
        {
            var state = ApplyMoves(new GameState(), moveList);

            var moves = state.GetAvailableMoves();

            Assert.That(moves, Is.Empty);
        }

        [TestCaseSource(nameof(unfinished))]
        public void GetAvailableMoves_WhenTheGameHasNotCompleted_ReturnsANonEmptyListOfMoves(string moveList)
        {
            var state = ApplyMoves(new GameState(), moveList);

            var moves = state.GetAvailableMoves();

            Assert.That(moves, Is.Not.Empty);
        }

        [Test]
        public void GetAvailableMoves_WhenTheGameStateIsNew_ReturnsMovesForTheFirstPlayer()
        {
            var state = new GameState();

            var moves = state.GetAvailableMoves();

            Assert.That(moves, Is.All.Property(nameof(Move.PlayerToken)).EqualTo(state.Players[0]));
        }

        [Test]
        public void GetAvailableMoves_WhenTheGameStateIsNew_ReturnsMovesWithinRange()
        {
            var state = new GameState();

            var moves = from m in state.GetAvailableMoves()
                        where m.X < 0 || m.X > 2 || m.Y < 0 || m.Y > 2
                        select m;

            Assert.That(moves, Is.Empty);
        }

        [Test]
        public void GetAvailableMoves_WhenTheGameStateIsNew_ReturnsNineUniqueMoves()
        {
            var state = new GameState();

            var moves = from m in state.GetAvailableMoves()
                        group m by new { m.X, m.Y } into g
                        select g.Key;

            Assert.That(moves.Count(), Is.EqualTo(9));
        }

        [TestCaseSource(nameof(unfinished))]
        public void GetWinners_WhenTheGameHasNotCompleted_ReturnsAnEmptyList(string moveList)
        {
            var state = ApplyMoves(new GameState(), moveList);

            var winners = state.GetWinners();

            Assert.That(winners, Is.Empty);
        }

        [TestCaseSource(nameof(draws))]
        public void GetWinners_WhenTheGameResultsInADraw_ReturnsAnEmptyList(string moveList)
        {
            var state = ApplyMoves(new GameState(), moveList);

            var winners = state.GetWinners();

            Assert.That(winners, Is.Empty);
        }

        [TestCaseSource(nameof(player1Wins))]
        public void GetWinners_WhenTheGameResultsInAWinForPlayer1_ReturnsAListContainingOnlyPlayer1(string moveList)
        {
            var state = ApplyMoves(new GameState(), moveList);

            var winner = state.GetWinners().SingleOrDefault();

            var player1 = state.Players[0];
            Assert.That(winner, Is.EqualTo(player1));
        }

        [TestCaseSource(nameof(player2Wins))]
        public void GetWinners_WhenTheGameResultsInAWinForPlayer2_ReturnsAListContainingOnlyPlayer2(string moveList)
        {
            var state = ApplyMoves(new GameState(), moveList);

            var winner = state.GetWinners().SingleOrDefault();

            var player2 = state.Players[1];
            Assert.That(winner, Is.EqualTo(player2));
        }

        internal static TGameState ApplyMoves<TGameState>(TGameState state, string moveList)
            where TGameState : IGameState<Move>
        {
            var moves = from move in moveList.Split(';')
                        let parts = move.Split(',')
                        let x = int.Parse(parts[0].Trim())
                        let y = int.Parse(parts[1].Trim())
                        select new { x, y };

            return moves.Aggregate(state, (s, m) => (TGameState)s.MakeMove(s.GetAvailableMoves().Single(a => a.X == m.x && a.Y == m.y)));
        }
    }
}
