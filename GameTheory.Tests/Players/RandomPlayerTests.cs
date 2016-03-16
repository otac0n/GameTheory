﻿// Copyright © 2016 John Gietzen.  All Rights Reserved.
// This source is subject to the MIT license.
// Please see license.md for more information.

namespace GameTheory.Tests.Players
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using GameTheory.Players;
    using NUnit.Framework;

    [TestFixture]
    public class RandomPlayerTests
    {
        [Test]
        public async Task GetMove_Always_ReturnsARandomMove()
        {
            const int Samples = 1000;
            const int Moves = 3;
            const double Expected = (double)Samples / Moves;

            var gameState = new StubGameState();
            using (var player = new RandomPlayer<StubGameState.Move>(gameState.Players[0]))
            {
                gameState.Moves = Enumerable.Range(0, Moves).Select(i => new StubGameState.Move(player.PlayerToken, "Move " + (char)('A' + i))).ToList();

                var moves = gameState.Moves.ToDictionary(m => m.Value, m => 0);
                for (int i = 0; i < Samples; i++)
                {
                    var move = await player.ChooseMove(gameState, CancellationToken.None);
                    moves[move.Value]++;
                }

                Assert.That(moves, Is.All.Property("Value").EqualTo(Expected).Within(3.29 * Math.Sqrt(Expected)));
            }
        }
    }
}
