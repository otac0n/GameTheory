// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Players.MaximizingPlayers
{
    using System.Linq;
    using Games.TicTacToe;

    /// <summary>
    /// A maximizing player for the game of <see cref="GameState">Tic tac toe</see>.
    /// </summary>
    public class TicTacToeMaximizingPlayer : MaximizingPlayer<Move, double>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TicTacToeMaximizingPlayer"/> class.
        /// </summary>
        /// <param name="playerToken">The token that represents the player.</param>
        /// <param name="minPly">The minimum number of ply to think ahead.</param>
        public TicTacToeMaximizingPlayer(PlayerToken playerToken, int minPly = 1)
            : base(playerToken, new ScoringMetric(), minPly)
        {
        }

        /// <summary>
        /// Provides a scoring metric for Tic-tac-toe.
        /// </summary>
        public class ScoringMetric : IScoringMetric
        {
            /// <inheritdoc/>
            public double CombineScores(double[] scores, double[] weights) =>
                scores.Select((s, i) => s * weights[i]).Sum() / weights.Sum();

            /// <inheritdoc/>
            public int Compare(double x, double y) =>
                x.CompareTo(y);

            /// <inheritdoc/>
            public double Difference(double playerScore, double opponentScore) =>
                playerScore - opponentScore;

            /// <inheritdoc/>
            public double Score(IGameState<Move> gameState, PlayerToken playerToken) =>
                gameState.GetWinners().Any(w => w == playerToken) ? 1 : 0;
        }
    }
}
