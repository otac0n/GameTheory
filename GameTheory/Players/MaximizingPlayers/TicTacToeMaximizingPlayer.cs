// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Players.MaximizingPlayers
{
    using System;
    using System.Linq;
    using Games.TicTacToe;

    /// <summary>
    /// A maximizing player for the game of <see cref="GameState">Tic tac toe</see>.
    /// </summary>
    public sealed class TicTacToeMaximizingPlayer : MaximizingPlayer<Move, double>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TicTacToeMaximizingPlayer"/> class.
        /// </summary>
        /// <param name="playerToken">The token that represents the player.</param>
        /// <param name="minPly">The minimum number of ply to think ahead.</param>
        public TicTacToeMaximizingPlayer(PlayerToken playerToken, int minPly = 6)
            : base(playerToken, new ScoringMetric(), minPly)
        {
        }

        /// <summary>
        /// Provides a scoring metric for Tic-tac-toe.
        /// </summary>
        private class ScoringMetric : IScoringMetric
        {
            /// <inheritdoc/>
            public double CombineScores(IWeighted<double>[] scores) =>
                scores.Sum(s => s.Value * s.Weight) / scores.Sum(s => s.Weight);

            /// <inheritdoc/>
            public int Compare(double x, double y) =>
                x.CompareTo(y);

            /// <inheritdoc/>
            public double Difference(double playerScore, double opponentScore) =>
                playerScore - opponentScore;

            /// <inheritdoc/>
            public double Score(IGameState<Move> state, PlayerToken playerToken)
            {
                if (state == null)
                {
                    throw new ArgumentNullException(nameof(state));
                }

                return state.GetWinners().Any(w => w == playerToken) ? 1 : 0;
            }
        }
    }
}
