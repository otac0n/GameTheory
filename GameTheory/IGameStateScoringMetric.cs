// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System.Collections.Generic;

    /// <summary>
    /// A scoring metric that can score an entire game state at once.
    /// </summary>
    /// <typeparam name="TMove">The type of move in the game state.</typeparam>
    /// <typeparam name="TScore">The type used to keep track of the score.</typeparam>
    public interface IGameStateScoringMetric<TMove, TScore> : IScoringMetric<PlayerState<TMove>, TScore>
        where TMove : IMove
    {
        /// <summary>
        /// Scores the specified state for all players.
        /// </summary>
        /// <param name="state">The game state to score.</param>
        /// <returns>A dictionary containing scores for all players in the game state.</returns>
        IDictionary<PlayerToken, TScore> Score(IGameState<TMove> state);
    }
}
