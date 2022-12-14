// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the contract for the implementation of a player.
    /// </summary>
    /// <typeparam name="TGameState">The type of game states that the player will evaluate.</typeparam>
    /// <typeparam name="TMove">The type of moves that the player will choose.</typeparam>
    public interface IPlayer<TGameState, TMove> : IDisposable
        where TGameState : IGameState<TMove>
        where TMove : IMove
    {
        /// <summary>
        /// Subscribes
        /// </summary>
        event EventHandler<MessageSentEventArgs> MessageSent;

        /// <summary>
        /// Gets the <see cref="PlayerToken"/> that represents the player.
        /// </summary>
        PlayerToken PlayerToken { get; }

        /// <summary>
        /// Instructs the player to choose a move from the specified game state as an asynchronous operation using a task.
        /// </summary>
        /// <param name="state">The <typeparamref name="TGameState"/> for which the player will choose a move.</param>
        /// <param name="cancel">A <see cref="CancellationToken"/> that notifies a player if the request for a move is cancelled.</param>
        /// <returns>A task representing the ongoing operation.</returns>
        Task<Maybe<TMove>> ChooseMove(TGameState state, CancellationToken cancel);
    }
}
