// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Players.MaximizingPlayers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Games.TwentyFortyEight;
    using GameTheory.Players.MaximizingPlayer;

    /// <summary>
    /// A maximizing player for the game of <see cref="GameState">Tic tac toe</see>.
    /// </summary>
    public sealed class TwentyFortyEightMaximizingPlayer : MaximizingPlayer<Move, double>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TwentyFortyEightMaximizingPlayer"/> class.
        /// </summary>
        /// <param name="playerToken">The token that represents the player.</param>
        /// <param name="minPly">The minimum number of ply to think ahead.</param>
        public TwentyFortyEightMaximizingPlayer(PlayerToken playerToken, int minPly)
            : base(playerToken, new PlayerScoringMetric(), minPly)
        {
        }

        /// <summary>
        /// Provides a scoring metric for Tic-tac-toe.
        /// </summary>
        private class PlayerScoringMetric : IPlayerScoringMetric
        {
            private static readonly double[] Pow = Enumerable.Range(0, GameState.Size * GameState.Size).Select(value => Math.Pow(10, value)).ToArray();

            /// <inheritdoc/>
            public double Combine(IWeighted<double>[] scores) =>
                ScoringMetric.Combine(scores);

            /// <inheritdoc/>
            public int Compare(double x, double y) =>
                x.CompareTo(y);

            /// <inheritdoc/>
            public double Difference(double playerScore, double opponentScore) =>
                playerScore - opponentScore;

            /// <inheritdoc/>
            public double Score(PlayerState playerState)
            {
                var state = (GameState)playerState.GameState;
                if (state.Players[0] == playerState.PlayerToken)
                {
                    var sums = new[] { 0.0, 0.0, 0.0, 0.0 };

                    var cells = new List<byte>();
                    for (var y = 0; y < GameState.Size; y++)
                    {
                        for (var x = 0; x < GameState.Size; x++)
                        {
                            var value = state[x, y];
                            if (value != 0)
                            {
                                var l = x == 0 ? -1 : state[x - 1, y];
                                var u = y == 0 ? -1 : state[x, y - 1];
                                var r = x == GameState.Size - 1 ? -1 : state[x + 1, y];
                                var d = y == GameState.Size - 1 ? -1 : state[x, y + 1];

                                const int Lower = -1, Higher = 1, Equal = 0, Edge = 2, Empty = 3;
                                var l2 = l == -1 ? Edge : l == 0 ? Empty : (l > value ? Higher : l < value ? Lower : Equal);
                                var u2 = u == -1 ? Edge : u == 0 ? Empty : (u > value ? Higher : u < value ? Lower : Equal);
                                var r2 = r == -1 ? Edge : r == 0 ? Empty : (r > value ? Higher : r < value ? Lower : Equal);
                                var d2 = d == -1 ? Edge : d == 0 ? Empty : (d > value ? Higher : d < value ? Lower : Equal);

                                var lr = 1.0;
                                var ud = 1.0;

                                if ((l2 == Lower && r2 == Lower) || ((l2 == Higher || l2 == Edge) && (r2 == Higher || r2 == Edge)))
                                {
                                    lr = 0.9;
                                }

                                if ((u2 == Lower && d2 == Lower) || ((u2 == Higher || u2 == Edge) && (d2 == Higher || d2 == Edge)))
                                {
                                    ud = 0.9;
                                }

                                var exp = Pow[value];
                                sums[0] += ud * exp / (1 + x);
                                sums[1] += lr * exp / (1 + y);
                                sums[2] += ud * exp / (4 - x);
                                sums[3] += lr * exp / (4 - y);
                            }
                        }
                    }

                    return sums.Max();
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}
