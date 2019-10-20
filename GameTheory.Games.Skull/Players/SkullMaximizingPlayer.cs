// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Skull.Players
{
    using System;
    using GameTheory.Players.MaximizingPlayer;

    /// <summary>
    /// Provides a maximizing player for the game of <see cref="GameState">Skull</see>.
    /// </summary>
    public sealed class SkullMaximizingPlayer : MaximizingPlayer<Move, ResultScore<double>>
    {
        private static readonly IGameStateScoringMetric<Move, double> Metric =
            ScoringMetric.Create<Move>(Score);

        private TimeSpan minThinkTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="SkullMaximizingPlayer"/> class.
        /// </summary>
        /// <param name="playerToken">The token that represents the player.</param>
        /// <param name="minPly">The minimum number of ply to think ahead.</param>
        /// <param name="thinkSeconds">The minimum number of seconds to think.</param>
        /// <param name="misereMode">A value indicating whether or not to play misère.</param>
        public SkullMaximizingPlayer(PlayerToken playerToken, int minPly = 9, int thinkSeconds = 5, bool misereMode = false)
            : base(playerToken, ResultScoringMetric.Create(Metric, misereMode), minPly)
        {
            this.minThinkTime = TimeSpan.FromSeconds(Math.Max(1, thinkSeconds) - 0.1);
        }

        /// <inheritdoc />
        protected override int InitialSamples => 100;

        /// <inheritdoc />
        protected override TimeSpan? MinThinkTime => this.minThinkTime;

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
