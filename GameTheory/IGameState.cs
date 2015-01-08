// -----------------------------------------------------------------------
// <copyright file="IGameState.cs" company="(none)">
//   Copyright © 2014 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents the contract of a game state.
    /// </summary>
    /// <typeparam name="TMove">The type of object that represents a move.</typeparam>
    public interface IGameState<TMove> where TMove : IMove
    {
        /// <summary>
        /// Gets a list of players in the current game state.
        /// </summary>
        /// <returns>The list of players in the current game state.</returns>
        IReadOnlyList<PlayerToken> Players { get; }

        /// <summary>
        /// Returns a collection of moves available to the specified player.
        /// </summary>
        /// <param name="player">The player whose moves will be retrieved.</param>
        /// <returns>An enumerable list of moves available to the specified player.</returns>
        IReadOnlyCollection<TMove> GetAvailableMoves(PlayerToken player);

        /// <summary>
        /// Applies the specified move to the given game state and returns the result.
        /// </summary>
        /// <param name="move">The move to apply.</param>
        /// <returns>The modified game state.</returns>
        IGameState<TMove> MakeMove(TMove move);
    }
}
