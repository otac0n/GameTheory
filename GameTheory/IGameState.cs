﻿// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents the contract of a game state.
    /// </summary>
    /// <typeparam name="TMove">The type of object that represents a move.</typeparam>
    public interface IGameState<TMove>
        where TMove : IMove
    {
        /// <summary>
        /// Gets a list of players in the current game state.
        /// </summary>
        /// <returns>The list of players in the current game state.</returns>
        IReadOnlyList<PlayerToken> Players { get; }

        /// <summary>
        /// Returns a collection of moves available to the specified player.
        /// </summary>
        /// <returns>An enumerable list of moves available to the specified player.</returns>
        IReadOnlyCollection<TMove> GetAvailableMoves();

        /// <summary>
        /// Returns any winners for the current game state.
        /// </summary>
        /// <returns>The list of players who can be considered winners.</returns>
        /// <remarks>
        /// More than one player may be listed, if the game allows for multiple winners.
        /// If the game results in a draw, this collection will be empty.
        /// </remarks>
        IReadOnlyCollection<PlayerToken> GetWinners();

        /// <summary>
        /// Applies the specified move to the given game state and returns the result.
        /// </summary>
        /// <param name="move">The move to apply.</param>
        /// <returns>The modified game state.</returns>
        IGameState<TMove> MakeMove(TMove move);

        /// <summary>
        /// Yields an enumerable collection of possible outcomes if the selected move is applied.
        /// </summary>
        /// <remarks>For deterministic moves, this should return a single element with the same value as a call to <see cref="MakeMove(TMove)"/>.</remarks>
        /// <param name="move">The move to evaluate.</param>
        /// <returns>The enumerable collection of possible outcomes.</returns>
        IEnumerable<IWeighted<IGameState<TMove>>> GetOutcomes(TMove move);

        /// <summary>
        /// Returns a view of the game with only information available to the specified player.
        /// </summary>
        /// <param name="playerToken">The player whose view will be returned.</param>
        /// <returns>A view of the game from the specified player's view.</returns>
        IGameState<TMove> GetView(PlayerToken playerToken);
    }
}
