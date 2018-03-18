﻿// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Players.MaximizingPlayers
{
    using Games.Skull;
    using GameTheory.Players.MaximizingPlayer;

    /// <summary>
    /// Provides a maximizing player for the game of <see cref="GameState">Skull</see>.
    /// </summary>
    public sealed class SkullMaximizingPlayer : MaximizingPlayer<Move, ResultScore<double>>
    {
        private static readonly ResultScoringMetric<Move, double> Metric =
            new ResultScoringMetric<Move, double>(ScoringMetric.Create<Move>(Score));

        /// <summary>
        /// Initializes a new instance of the <see cref="SkullMaximizingPlayer"/> class.
        /// </summary>
        /// <param name="playerToken">The token that represents the player.</param>
        /// <param name="minPly">The minimum number of ply to think ahead.</param>
        public SkullMaximizingPlayer(PlayerToken playerToken, int minPly)
            : base(playerToken, Metric, minPly)
        {
        }

        private static double Score(PlayerState<Move> playerState)
        {
            var state = (GameState)playerState.GameState;
            var inventory = state.Inventory[playerState.PlayerToken];
            var discardedSkull = inventory.Discards[Card.Skull];
            var discardedFlowers = inventory.Discards[Card.Flower];
            return inventory.ChallengesWon + (0.3 - 0.1 * discardedFlowers) + (discardedSkull == 0 && discardedFlowers < 3 ? 0.1 : 0);
        }
    }
}
