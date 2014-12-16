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
        /// Returns a finite, enumerable collection of moves.
        /// </summary>
        /// <returns>An enumerable list of moves available.</returns>
        IEnumerable<TMove> GetAvailableMoves();

        /// <summary>
        /// Applies the specified move to the given game state and returns the result.
        /// </summary>
        /// <param name="move">The move to apply.</param>
        /// <returns>The modified game state.</returns>
        IGameState<TMove> Move(TMove move);
    }
}
