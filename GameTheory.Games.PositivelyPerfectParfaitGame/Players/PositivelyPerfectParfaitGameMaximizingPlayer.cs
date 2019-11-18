// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.PositivelyPerfectParfaitGame.Players
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GameTheory.Players.MaximizingPlayer;

    /// <summary>
    /// Provides a maximizing player for the game of <see cref="GameState">Positively Perfect Parfait Game</see>.
    /// </summary>
    public class PositivelyPerfectParfaitGameMaximizingPlayer : MaximizingPlayer<GameState, Move, ResultScore<double>>
    {
        private static readonly IGameStateScoringMetric<GameState, Move, double> Metric =
            ScoringMetric.Create((PlayerState<GameState, Move> s) => s.GameState.Parfaits[s.PlayerToken].Flavors.Keys.Count());

        private TimeSpan minThinkTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="PositivelyPerfectParfaitGameMaximizingPlayer"/> class.
        /// </summary>
        /// <param name="playerToken">The token that represents the player.</param>
        /// <param name="minPly">The minimum number of ply to think ahead.</param>
        /// <param name="thinkSeconds">The minimum number of seconds to think.</param>
        /// <param name="misereMode">A value indicating whether or not to play misère.</param>
        public PositivelyPerfectParfaitGameMaximizingPlayer(PlayerToken playerToken, int minPly = 8, int thinkSeconds = 5, bool misereMode = false)
            : base(playerToken, ResultScoringMetric.Create(Metric, misereMode), minPly)
        {
            this.minThinkTime = TimeSpan.FromSeconds(Math.Max(1, thinkSeconds) - 0.1);
        }

        /// <inheritdoc />
        protected override TimeSpan? MinThinkTime => this.minThinkTime;

        /// <inheritdoc/>
        protected override ResultScore<double> GetLead(IDictionary<PlayerToken, ResultScore<double>> score, GameState state, PlayerToken player)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (score == null)
            {
                throw new ArgumentNullException(nameof(score));
            }

            if (state.PlayOut)
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
