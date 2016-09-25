// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Tests.Strategies
{
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
            var gameState = Games.TicTacToeTests.ApplyMoves(new GameState(), moveList);
            using (var strategy = new ImmediateWinStrategy<Move>(gameState.ActivePlayer))
            {
                var maybeMove = await strategy.ChooseMove(gameState, CancellationToken.None);
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
            var gameState = Games.TicTacToeTests.ApplyMoves(new GameState(), moveList);
            using (var strategy = new ImmediateWinStrategy<Move>(gameState.ActivePlayer))
            {
                var maybeMove = await strategy.ChooseMove(gameState, CancellationToken.None);
                Assert.That(maybeMove.HasValue, Is.False);
            }
        }
    }
}
