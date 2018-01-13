﻿// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Players.MaximizingPlayers
{
    using System.Linq;
    using Games.Lotus;

    /// <summary>
    /// A maximizing player for the game of <see cref="GameState">Lotus</see>.
    /// </summary>
    public class LotusMaximizingPlayer : MaximizingPlayer<Move, double>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LotusMaximizingPlayer"/> class.
        /// </summary>
        /// <param name="playerToken">The token that represents the player.</param>
        /// <param name="minPly">The minimum number of ply to think ahead.</param>
        public LotusMaximizingPlayer(PlayerToken playerToken, int minPly)
            : base(playerToken, new PlayerScoringMetric(), minPly)
        {
        }

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
                var state = (GameState)playerState.GameState;
                double score = state.GetScore(playerState.PlayerToken);
                if (state.Phase != Phase.End)
                {
                    foreach (var flower in state.Field.Values)
                    {
                        if (!flower.Petals.IsEmpty)
                        {
                            var controllingPlayers = GameState.GetControllingPlayers(flower);
                            if (controllingPlayers.Contains(playerState.PlayerToken))
                            {
                                score += (double)flower.Petals.Count / controllingPlayers.Count;
                            }
                        }
                    }
                }

                return score;
            }
        }
    }
}
