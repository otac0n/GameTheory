// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Extensions for all implementations of <see cref="IGameState{TMove}"/>.
    /// </summary>
    public static class GameStateExtensions
    {
        /// <summary>
        /// Gets all moves for all players in the specified <see cref="IGameState{TMove}"/>.
        /// </summary>
        /// <typeparam name="TMove">The type of object that represents a move.</typeparam>
        /// <param name="state">The game state for which to retrieve all moves.</param>
        /// <param name="playerToken">The player whose moves will be retrieved.</param>
        /// <returns>All moves for all players in the specified game state.</returns>
        public static List<TMove> GetAvailableMoves<TMove>(this IGameState<TMove> state, PlayerToken playerToken)
            where TMove : IMove
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (playerToken == null)
            {
                throw new ArgumentNullException(nameof(playerToken));
            }

            return state.GetAvailableMoves().Where(m => m.PlayerToken == playerToken).ToList();
        }
    }
}
