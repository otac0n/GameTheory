// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Players.MaximizingPlayer
{
    /// <summary>
    /// Provides initialization methods for <see cref="ResultScoringMetric{TGameState, TMove, TScore}"/> and its subclasses.
    /// </summary>
    public static class ResultScoringMetric
    {
        /// <summary>
        /// Creates a <see cref="ResultScoringMetric{TGameState, TMove, TScore}"/>.
        /// </summary>
        /// <typeparam name="TGameState">The type of game state to score.</typeparam>
        /// <typeparam name="TMove">The type of move in the game state.</typeparam>
        /// <typeparam name="TScore">The type used to keep track of the inner score.</typeparam>
        /// <param name="scoringMetric">The inner scoring metric.</param>
        /// <param name="misereMode">A value indicating wheter or not to play misère.</param>
        /// <returns>The requested <see cref="ResultScoringMetric{TGameState, TMove, TScore}"/>.</returns>
        public static ResultScoringMetric<TGameState, TMove, TScore> Create<TGameState, TMove, TScore>(IGameStateScoringMetric<TGameState, TMove, TScore> scoringMetric, bool misereMode = false)
            where TGameState : IGameState<TMove>
            where TMove : IMove
        {
            return misereMode ? new MisereResultScoringMetric<TGameState, TMove, TScore>(scoringMetric) : new ResultScoringMetric<TGameState, TMove, TScore>(scoringMetric);
        }
    }
}
