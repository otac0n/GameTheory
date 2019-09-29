// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Tests.Games.Chess
{
    using System.Linq;
    using GameTheory.Games.Chess;
    using GameTheory.Games.Chess.Players;
    using NUnit.Framework;

    public class PlayerTests
    {
        [Test]
        public void ScoreForWhite_WithFurtherAdvancedPawns_YieldsHigherScore()
        {
            var positions = new[]
            {
                "8/8/8/8/8/8/k1K4P/8 w - - 0 1",
                "8/8/8/8/8/7P/k1K5/8 w - - 0 1",
                "8/8/8/8/7P/8/k1K5/8 w - - 0 1",
                "8/8/8/7P/8/8/k1K5/8 w - - 0 1",
                "8/8/7P/8/8/8/k1K5/8 w - - 0 1",
                "8/7P/8/8/8/8/k1K5/8 w - - 0 1",
            };

            var results = positions.Select((p, i) =>
            {
                var state = new GameState(p);
                return new
                {
                    Index = i,
                    Position = p,
                    GameState = state,
                    Score = ChessMaximizingPlayer.Score(new PlayerState<Move>(state.Players[0], state)),
                };
            }).ToList();

            Assert.That(results, Is.EqualTo(results.AsEnumerable().Reverse().OrderBy(r => r.Score)));
        }

        [Test]
        public void ScoreForBlack_WithFurtherAdvancedPawns_YieldsHigherScore()
        {
            var positions = new[]
            {
                "8/p4k1K/8/8/8/8/8/8 b - - 0 1",
                "8/5k1K/p7/8/8/8/8/8 b - - 0 1",
                "8/5k1K/8/p7/8/8/8/8 b - - 0 1",
                "8/5k1K/8/8/p7/8/8/8 b - - 0 1",
                "8/5k1K/8/8/8/p7/8/8 b - - 0 1",
                "8/5k1K/8/8/8/8/p7/8 b - - 0 1",
            };

            var results = positions.Select((p, i) =>
            {
                var state = new GameState(p);
                return new
                {
                    Index = i,
                    Position = p,
                    GameState = state,
                    Score = ChessMaximizingPlayer.Score(new PlayerState<Move>(state.Players[1], state)),
                };
            }).ToList();

            Assert.That(results, Is.EqualTo(results.AsEnumerable().Reverse().OrderBy(r => r.Score)));
        }
    }
}
