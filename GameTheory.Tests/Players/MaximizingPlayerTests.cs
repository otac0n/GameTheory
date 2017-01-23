// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Tests.Players
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using GameTheory.Games.TicTacToe;
    using GameTheory.Players;
    using NUnit.Framework;

    [TestFixture]
    public class MaximizingPlayerTests
    {
        [Test]
        public async Task WhenGoingFirst_Never_LosesToDuhPlayer()
        {
            const int Samples = 1000;

            var wins = new int[2];

            for (int i = 0; i < Samples; i++)
            {
                var endState = await GameUtils.PlayGame(new GameState(), playerTokens => new IPlayer<Move>[]
                {
                    new MaximizingPlayer<Move, double>(playerTokens[0], new ScoringMetric(), minPly: 5),
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

            for (int i = 0; i < Samples; i++)
            {
                var endState = await GameUtils.PlayGame(new GameState(), playerTokens => new IPlayer<Move>[]
                {
                    new DuhPlayer<Move>(playerTokens[0]),
                    new MaximizingPlayer<Move, double>(playerTokens[1], new ScoringMetric(), minPly: 6),
                });

                var players = endState.Players.ToArray();
                foreach (var winner in endState.GetWinners())
                {
                    wins[Array.IndexOf(players, winner)]++;
                }
            }

            Assert.That(wins[0], Is.Zero);
        }

        private class ScoringMetric : MaximizingPlayer<Move, double>.IScoringMetric
        {
            public double CombineScores(double[] scores, double[] weights) =>
                scores.Select((s, i) => s * weights[i]).Sum() / weights.Sum();

            public double Difference(double playerScore, double opponentScore) =>
                playerScore - opponentScore;

            public int Compare(double x, double y) =>
                x.CompareTo(y);

            public double Score(IGameState<Move> gameState, PlayerToken playerToken) =>
                gameState.GetWinners().Any(w => w == playerToken) ? 1 : 0;
        }
    }
}
