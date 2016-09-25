// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

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
        /// <returns>All moves for all players in the specified game state.</returns>
        public static List<TMove> GetAvailableMoves<TMove>(this IGameState<TMove> state)
            where TMove : IMove
        {
            Contract.Requires(state != null);

            var moves = new List<TMove>();

            foreach (var player in state.Players)
            {
                moves.AddRange(state.GetAvailableMoves(player));
            }

            return moves;
        }
    }
}
