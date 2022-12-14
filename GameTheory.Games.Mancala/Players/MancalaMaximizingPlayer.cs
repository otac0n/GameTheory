// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Mancala.Players
{
    using System;
    using GameTheory.GameTree;
    using GameTheory.GameTree.Caches;
    using GameTheory.Players.MaximizingPlayer;

    /// <summary>
    /// Provides a maximizing player for the game of <see cref="GameState">Mancala</see>.
    /// </summary>
    public class MancalaMaximizingPlayer : MaximizingPlayer<GameState, Move, ResultScore<double>>
    {
        private static readonly IGameStateScoringMetric<GameState, Move, double> Metric =
            ScoringMetric.Create<GameState, Move>(Score);

        private TimeSpan minThinkTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="MancalaMaximizingPlayer"/> class.
        /// </summary>
        /// <param name="playerToken">The token that represents the player.</param>
        /// <param name="minPly">The minimum number of ply to think ahead.</param>
        /// <param name="thinkSeconds">The minimum number of seconds to think.</param>
        /// <param name="misereMode">A value indicating whether or not to play misère.</param>
        public MancalaMaximizingPlayer(PlayerToken playerToken, int minPly = 9, int thinkSeconds = 2, bool misereMode = false)
            : base(playerToken, ResultScoringMetric.Create(Metric, misereMode), minPly)
        {
            this.minThinkTime = TimeSpan.FromSeconds(Math.Max(1, thinkSeconds) - 0.1);
        }

        /// <inheritdoc />
        protected override TimeSpan? MinThinkTime => this.minThinkTime;

        /// <inheritdoc />
        protected override IGameStateCache<GameState, Move, ResultScore<double>> MakeCache() => new NullCache<GameState, Move, ResultScore<double>>();

        private static double Score(PlayerState<GameState, Move> playerState)
        {
            var state = playerState.GameState;
            return state[state.GetPlayerIndexOffset(state.Players.IndexOf(playerState.PlayerToken)) + state.BinsPerSide];
        }
    }
}
