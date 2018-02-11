﻿// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Players.MaximizingPlayers
{
    using System.Collections.Generic;
    using System.Linq;
    using Games.PositivelyPerfectParfaitGame;
    using GameTheory.Players.MaximizingPlayer;

    /// <summary>
    /// Provides a maximizing player for the game of <see cref="GameState">Splendor</see>.
    /// </summary>
    public class PositivelyPerfectParfaitGameMaximizingPlayer : MaximizingPlayer<Move, double>
    {
        private static readonly IScoringMetric<PlayerState, double> Metric = ScoringMetric.Create<PlayerState>(s => ((GameState)s.GameState).Parfaits[s.PlayerToken].Flavors.Keys.Count());

        /// <summary>
        /// Initializes a new instance of the <see cref="PositivelyPerfectParfaitGameMaximizingPlayer"/> class.
        /// </summary>
        /// <param name="playerToken">The token that represents the player.</param>
        /// <param name="minPly">The minimum number of ply to think ahead.</param>
        public PositivelyPerfectParfaitGameMaximizingPlayer(PlayerToken playerToken, int minPly = 8)
            : base(playerToken, Metric, minPly)
        {
        }

        /// <inheritdoc/>
        protected override ResultScore<double> GetLead(IDictionary<PlayerToken, ResultScore<double>> score, IGameState<Move> state, PlayerToken player)
        {
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
