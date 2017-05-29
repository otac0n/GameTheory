// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Players.MaximizingPlayers
{
    using System.Linq;
    using GameTheory.Games.Mancala;

    /// <summary>
    /// A maximizing player for the game of <see cref="GameState">Mancala</see>.
    /// </summary>
    public class MancalaMaximizingPlayer : MaximizingPlayer<Move, double>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MancalaMaximizingPlayer"/> class.
        /// </summary>
        /// <param name="playerToken">The token that represents the player.</param>
        /// <param name="minPly">The minimum number of ply to think ahead.</param>
        public MancalaMaximizingPlayer(PlayerToken playerToken, int minPly = 6)
            : base(playerToken, new ScoringMetric(), minPly)
        {
        }

        private class ScoringMetric : IScoringMetric
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
            public double Score(IGameState<Move> state, PlayerToken playerToken)
            {
                var gameState = (GameState)state;
                var sum = 0D;
                var playerOffset = gameState.GetPlayerIndexOffset(playerToken);
                var binsPerSide = gameState.BinsPerSide;
                var discountThreshold = binsPerSide;
                var discountFactor = 0.5D;
                for (var i = 0; i <= binsPerSide; i++)
                {
                    var value = gameState.Board[i + playerOffset];
                    if (i == binsPerSide || value < discountThreshold)
                    {
                        sum += value;
                    }
                    else
                    {
                        sum += discountThreshold + (value - discountThreshold) * discountFactor;
                    }
                }

                return sum;
            }
        }
    }
}
