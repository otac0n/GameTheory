// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Tests.Players
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using GameTheory.Games.TicTacToe;
    using GameTheory.Games.TicTacToe.Players.MaximizingPlayers;
    using NUnit.Framework;

    [TestFixture]
    public class MaximizingPlayerTests
    {
        private const int Samples = 1000;

        [Test]
        public async Task WhenGoingFirst_Never_LosesToDuhPlayer()
        {
            var wins = new int[2];

            for (var i = 0; i < Samples; i++)
            {
                var endState = await GameUtilities.PlayGame(new GameState(), playerTokens => new IPlayer<Move>[]
                {
                    new TicTacToeMaximizingPlayer(playerTokens[0], minPly: 6),
                    new DuhPlayer<Move>(playerTokens[1]),
                });

                var players = endState.Players.ToArray();
                foreach (var winner in endState.GetWinners())
                {
                    wins[Array.IndexOf(players, winner)]++;
                }
            }

            Assert.That(wins[1], Is.Zero);
        }

        [Test]
        public async Task WhenGoingSecond_Never_LosesToDuhPlayer()
        {
            var wins = new int[2];

            for (var i = 0; i < Samples; i++)
            {
                var endState = await GameUtilities.PlayGame(new GameState(), playerTokens => new IPlayer<Move>[]
                {
                    new DuhPlayer<Move>(playerTokens[0]),
                    new TicTacToeMaximizingPlayer(playerTokens[1], minPly: 6),
                });

                var players = endState.Players.ToArray();
                foreach (var winner in endState.GetWinners())
                {
                    wins[Array.IndexOf(players, winner)]++;
                }
            }

            Assert.That(wins[0], Is.Zero);
        }

        [Test]
        public async Task WhenLossCanBeAverted_Always_AvoidsImmediateLoss(
            [Values("(0, 0);(1, 1);(2, 2);(2, 0);(0, 2)", "(0, 0);(1, 1);(2, 2);(1, 2);(1, 0)")] string moves,
            [Values(2, 3, 4, 5, 6)] int ply)
        {
            await InitializeStateAndAssertNextMove(moves, ply);
        }

        [Test]
        public async Task WhenLossCannotBeAverted_Always_AvoidsImmediateLoss(
            [Values("(2, 1);(2, 2);(0, 1);(1, 1);(0, 0)", "(2, 1);(2, 2);(2, 0);(1, 2);(0, 2)")] string moves,
            [Values(2, 3, 4, 5, 6)] int ply)
        {
            await InitializeStateAndAssertNextMove(moves, ply);
        }

        [Test]
        public async Task WhenMultipleWinsAreAvailable_Always_ChoosesTheSoonestWin(
            [Values("(0, 0);(0, 1);(2, 0);(2, 1);(1, 0)", "(0, 0);(0, 2);(2, 0);(0, 1);(1, 0)")] string moves,
            [Values(2, 3, 4, 5, 6)] int ply)
        {
            await InitializeStateAndAssertNextMove(moves, ply);
        }

        private static async Task InitializeStateAndAssertNextMove(string moves, int ply)
        {
            var state = new GameState();
            var playerX = state.Players[0];
            var playerO = state.Players[1];
            var i = 0;
            PlayerToken playerToken() => i++ % 2 == 0 ? playerX : playerO;
            var movesArray = moves.Split(';');
            var nextMove = movesArray.Last();
            Array.Resize(ref movesArray, movesArray.Length - 1);
            state = (GameState)movesArray.Aggregate((IGameState<Move>)state, (newState, move) => newState.PlayMove(playerToken(), m => m.ToString() == move));

            var token = playerToken();
            var success = 0;
            for (var samples = Samples; samples > 0; samples--)
            {
                var player = new TicTacToeMaximizingPlayer(token, minPly: ply);
                var cancel = new CancellationTokenSource();
                var chosenMove = await player.ChooseMove(state, cancel.Token);
                if (chosenMove.Value.ToString() == nextMove)
                {
                    success++;
                }
            }

            Assert.That(success, Is.EqualTo(Samples));
        }
    }
}
