// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

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

            var state = new StubGameState();
            using (var player = new RandomPlayer<StubGameState.Move>(state.Players[0]))
            {
                state.Moves = Enumerable.Range(0, Moves).Select(i => new StubGameState.Move(player.PlayerToken, "Move " + (char)('A' + i))).ToList();

                var moves = state.Moves.ToDictionary(m => m.Value, m => 0);
                for (int i = 0; i < Samples; i++)
                {
                    var move = (await player.ChooseMove(state, CancellationToken.None)).Value;
                    moves[move.Value]++;
                }

                Assert.That(moves, Is.All.Property("Value").EqualTo(Expected).Within(3.29 * Math.Sqrt(Expected)));
            }
        }
    }
}
