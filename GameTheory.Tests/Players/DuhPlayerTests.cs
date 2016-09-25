// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Tests.Players
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using GameTheory.Games.TicTacToe;
    using GameTheory.Players;
    using GameTheory.Strategies;
    using NUnit.Framework;

    [TestFixture]
    public class DuhPlayerTests
    {
        [Test]
        public async Task WhenGoingFirst_Usually_BeatsRandomPlayer()
        {
            const int Samples = 1000;

            var wins = new int[2];

            for (int i = 0; i < Samples; i++)
            {
                var endState = await GameUtils.PlayGame(new GameState(), playerTokens => new IPlayer<Move>[]
                {
                    new DuhPlayer<Move>(playerTokens[0]),
                    new RandomPlayer<Move>(playerTokens[1]),
                });

                var players = endState.Players.ToArray();
                foreach (var winner in endState.GetWinners())
                {
                    wins[Array.IndexOf(players, winner)]++;
                }
            }

            Assert.That(wins[0], Is.GreaterThan(wins[1]));
        }

        [Test]
        public async Task WhenGoingSecond_Usually_BeatsRandomPlayer()
        {
            const int Samples = 1000;

            var wins = new int[2];

            for (int i = 0; i < Samples; i++)
            {
                var endState = await GameUtils.PlayGame(new GameState(), playerTokens => new IPlayer<Move>[]
                {
                    new RandomPlayer<Move>(playerTokens[0]),
                    new DuhPlayer<Move>(playerTokens[1]),
                });

                var players = endState.Players.ToArray();
                foreach (var winner in endState.GetWinners())
                {
                    wins[Array.IndexOf(players, winner)]++;
                }
            }

            Assert.That(wins[1], Is.GreaterThan(wins[0]));
        }

        public class DuhPlayer<TMove> : StrategyPlayer<TMove>
            where TMove : IMove
        {
            public DuhPlayer(PlayerToken playerToken)
                : base(new RandomPlayer<TMove>(playerToken), new ImmediateWinStrategy<TMove>(playerToken))
            {
            }
        }
    }
}
