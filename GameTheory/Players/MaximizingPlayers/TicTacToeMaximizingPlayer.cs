// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Players.MaximizingPlayers
{
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
            : base(playerToken, new PlayerScoringMetric(), minPly)
        {
        }

        /// <summary>
        /// Provides a scoring metric for Tic-tac-toe.
        /// </summary>
        private class PlayerScoringMetric : IPlayerScoringMetric
        {
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
                var state = playerState.GameState;
                var playerToken = playerState.PlayerToken;
                return state.GetWinners().Any(w => w == playerToken) ? 1 : 0;
            }
        }
    }
}
