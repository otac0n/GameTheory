// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.LoveLetter.Players
{
    using System;
    using System.Linq;
    using GameTheory.Players.MaximizingPlayer;

    /// <summary>
    /// Provides a maximizing player for the game of <see cref="GameState">Love Letter</see>.
    /// </summary>
    public class LoveLetterMaximizingPlayer : MaximizingPlayer<GameState, Move, ResultScore<double>>
    {
        private static readonly IGameStateScoringMetric<GameState, Move, double> Metric =
            ScoringMetric.Create<GameState, Move>(Score);

        private TimeSpan minThinkTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoveLetterMaximizingPlayer"/> class.
        /// </summary>
        /// <param name="playerToken">The token that represents the player.</param>
        /// <param name="minPly">The minimum number of ply to think ahead.</param>
        /// <param name="thinkSeconds">The minimum number of seconds to think.</param>
        /// <param name="misereMode">A value indicating whether or not to play misère.</param>
        public LoveLetterMaximizingPlayer(PlayerToken playerToken, int minPly = 2, int thinkSeconds = 5, bool misereMode = false)
            : base(playerToken, ResultScoringMetric.Create(Metric, misereMode), minPly)
        {
            this.minThinkTime = TimeSpan.FromSeconds(Math.Max(1, thinkSeconds) - 0.1);
        }

        /// <inheritdoc/>
        protected override TimeSpan? MinThinkTime => this.minThinkTime;

        internal static double Score(PlayerState<GameState, Move> playerState)
        {
            var inventory = playerState.GameState.Inventory[playerState.PlayerToken];

            return inventory.Tokens + (inventory.Hand.Length > 0 ? inventory.Hand.Select(c => (int)c).Max() / 9.0 : 0);
        }
    }
}
