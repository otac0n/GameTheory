// Copyright © 2016 John Gietzen.  All Rights Reserved.
// This source is subject to the MIT license.
// Please see license.md for more information.

namespace GameTheory
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the contract for the implementation of a player.
    /// </summary>
    /// <typeparam name="TMove">The type of the moves that the player will choose.</typeparam>
    public interface IPlayer<TMove> : IDisposable
        where TMove : IMove
    {
        /// <summary>
        /// Instructs the player to choose a move from the specified game state as an asynchronous operation using a task.
        /// </summary>
        /// <param name="gameState">The <see cref="IGameState{TMove}"/> for which the player will choose a move.</param>
        /// <param name="cancel">A <see cref="CancellationToken"/> that notifies a player if the request for a move is cancelled.</param>
        /// <returns>A task representing the ongoing operation.</returns>
        Task<Maybe<TMove>> ChooseMove(IGameState<TMove> gameState, CancellationToken cancel);
    }
}
