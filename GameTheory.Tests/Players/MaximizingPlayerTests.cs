// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Tests.Players
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using GameTheory.Games.TicTacToe;
    using GameTheory.Players.MaximizingPlayers;
    using NUnit.Framework;

    [TestFixture]
    public class MaximizingPlayerTests
    {
        [Test]
        public async Task WhenGoingFirst_Never_LosesToDuhPlayer()
        {
            const int Samples = 1000;

            var wins = new int[2];

            for (var i = 0; i < Samples; i++)
            {
                var endState = await GameUtilities.PlayGame(new GameState(), playerTokens => new IPlayer<Move>[]
                {
                    new TicTacToeMaximizingPlayer(playerTokens[0], minPly: 5),
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
            const int Samples = 1000;

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
    }
}
