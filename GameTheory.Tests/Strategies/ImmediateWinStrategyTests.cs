// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Tests.Strategies
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using GameTheory.Games.TicTacToe;
    using GameTheory.Strategies;
    using NUnit.Framework;

    [TestFixture]
    public class ImmediateWinStrategyTests
    {
        [TestCase("0,0; 1,1; 0,1; 2,2", "0,2")]
        [TestCase("0,0; 1,1; 2,2; 0,2; 2,0; 1,0", "2,1")]
        [TestCase("0,0; 1,1; 2,2; 1,0; 2,0", "1,2")]
        public async Task GetMove_WhenThereIsAWinningMove_ReturnsAWinningMove(string moveList, string expectedMove)
        {
            var state = ApplyMoves(new GameState(), moveList);
            using (var strategy = new ImmediateWinStrategy<Move>())
            {
                var maybeMove = await strategy.ChooseMove(state, state.ActivePlayer, CancellationToken.None);
                var move = maybeMove.Value;
                Assert.That($"{move.X},{move.Y}", Is.EqualTo(expectedMove));
            }
        }

        [TestCase("0,0")]
        [TestCase("0,0; 1,1")]
        [TestCase("0,0; 1,1; 2,2")]
        [TestCase("0,0; 1,1; 2,2; 2,1")]
        [TestCase("0,0; 1,1; 2,2; 2,1; 0,1")]
        [TestCase("0,0; 1,1; 2,2; 2,1; 0,1; 0,2")]
        [TestCase("0,0; 1,1; 2,2; 2,1; 0,1; 0,2; 2,0")]
        [TestCase("0,0; 1,1; 2,2; 2,1; 0,1; 0,2; 2,0; 1,0")]
        public async Task GetMove_WhenThereIsNoWinningMove_ReturnsAnEmptyValue(string moveList)
        {
            var state = ApplyMoves(new GameState(), moveList);
            using (var strategy = new ImmediateWinStrategy<Move>())
            {
                var maybeMove = await strategy.ChooseMove(state, state.ActivePlayer, CancellationToken.None);
                Assert.That(maybeMove.HasValue, Is.False);
            }
        }

        internal static T ApplyMoves<T>(T state, string moveList)
            where T : IGameState<Move>
        {
            var moves = from move in moveList.Split(';')
                        let parts = move.Split(',')
                        let x = int.Parse(parts[0].Trim())
                        let y = int.Parse(parts[1].Trim())
                        select new { x, y };

            return (T)moves.Aggregate((IGameState<Move>)state, (s, m) => s.MakeMove(s.GetAvailableMoves().Single(a => a.X == m.x && a.Y == m.y)));
        }
    }
}
