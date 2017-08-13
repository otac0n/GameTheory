// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Players.MaximizingPlayers
{
    using System.Linq;
    using Games.CenturySpiceRoad;

    /// <summary>
    /// A maximizing player for the game of <see cref="GameState">CenturySpiceRoad</see>.
    /// </summary>
    public class CenturySpiceRoadMaximizingPlayer : MaximizingPlayer<Move, double>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CenturySpiceRoadMaximizingPlayer"/> class.
        /// </summary>
        /// <param name="playerToken">The token that represents the player.</param>
        /// <param name="minPly">The minimum number of ply to think ahead.</param>
        public CenturySpiceRoadMaximizingPlayer(PlayerToken playerToken, int minPly)
            : base(playerToken, new ScoringMetric(), minPly)
        {
        }

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
                var gameState = (GameState)state;
                return gameState.GetScore(playerToken);
            }
        }
    }
}
