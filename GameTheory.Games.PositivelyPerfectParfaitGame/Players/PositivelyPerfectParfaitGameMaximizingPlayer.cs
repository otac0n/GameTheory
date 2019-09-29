// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.PositivelyPerfectParfaitGame.Players.MaximizingPlayers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GameTheory.Players.MaximizingPlayer;

    /// <summary>
    /// Provides a maximizing player for the game of <see cref="GameState">Positively Perfect Parfait Game</see>.
    /// </summary>
    public class PositivelyPerfectParfaitGameMaximizingPlayer : MaximizingPlayer<Move, ResultScore<double>>
    {
        private static readonly IGameStateScoringMetric<Move, double> Metric =
            ScoringMetric.Create((PlayerState<Move> s) => ((GameState)s.GameState).Parfaits[s.PlayerToken].Flavors.Keys.Count());

        /// <summary>
        /// Initializes a new instance of the <see cref="PositivelyPerfectParfaitGameMaximizingPlayer"/> class.
        /// </summary>
        /// <param name="playerToken">The token that represents the player.</param>
        /// <param name="minPly">The minimum number of ply to think ahead.</param>
        /// <param name="misereMode">A value indicating whether or not to play misère.</param>
        public PositivelyPerfectParfaitGameMaximizingPlayer(PlayerToken playerToken, int minPly = 8, bool misereMode = false)
            : base(playerToken, ResultScoringMetric.Create(Metric, misereMode), minPly)
        {
        }

        /// <inheritdoc/>
        protected override ResultScore<double> GetLead(IDictionary<PlayerToken, ResultScore<double>> score, IGameState<Move> state, PlayerToken player)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (score == null)
            {
                throw new ArgumentNullException(nameof(score));
            }

            if (((GameState)state).PlayOut)
            {
                return score[player];
            }
            else
            {
                return base.GetLead(score, state, player);
            }
        }
    }
}