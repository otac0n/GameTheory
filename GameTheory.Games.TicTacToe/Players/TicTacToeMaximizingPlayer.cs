// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.TicTacToe.Players
{
    using System;
    using GameTheory.Players.MaximizingPlayer;

    /// <summary>
    /// Provides a maximizing player for the game of <see cref="GameState">Tic tac toe</see>.
    /// </summary>
    public sealed class TicTacToeMaximizingPlayer : MaximizingPlayer<GameState, Move, ResultScore<double>>
    {
        private static readonly IGameStateScoringMetric<GameState, Move, double> Metric =
            ScoringMetric.Create((PlayerState<GameState, Move> s) => 0);

        private TimeSpan minThinkTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="TicTacToeMaximizingPlayer"/> class.
        /// </summary>
        /// <param name="playerToken">The token that represents the player.</param>
        /// <param name="minPly">The minimum number of ply to think ahead.</param>
        /// <param name="thinkSeconds">The minimum number of seconds to think.</param>
        /// <param name="misereMode">A value indicating whether or not to play misère.</param>
        public TicTacToeMaximizingPlayer(PlayerToken playerToken, int minPly = 6, int thinkSeconds = 2, bool misereMode = false)
            : base(playerToken, ResultScoringMetric.Create(Metric, misereMode), minPly)
        {
            this.minThinkTime = TimeSpan.FromSeconds(Math.Max(1, thinkSeconds) - 0.1);
        }

        /// <inheritdoc />
        protected override TimeSpan? MinThinkTime => this.minThinkTime;
    }
}
