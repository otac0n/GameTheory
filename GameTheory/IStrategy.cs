// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the contract for the implementation of a strategy, which can be composed with other strategies or players.
    /// </summary>
    /// <typeparam name="TMove">The type of the moves that the strategy will choose.</typeparam>
    public interface IStrategy<TMove> : IDisposable
        where TMove : IMove
    {
        /// <summary>
        /// Instructs the strategy to choose a move from the specified collection as an asynchronous operation using a task.
        /// </summary>
        /// <param name="state">The <see cref="IGameState{TMove}"/> for which the player will choose a move.</param>
        /// <param name="playerToken">The player who is considered to be using this strategy.</param>
        /// <param name="moves">The list of moves available.</param>
        /// <param name="cancel">A <see cref="CancellationToken"/> that notifies a player if the request for a move is cancelled.</param>
        /// <returns>A task representing the ongoing operation.</returns>
        Task<Maybe<TMove>> ChooseMove(IGameState<TMove> state, PlayerToken playerToken, IReadOnlyCollection<TMove> moves, CancellationToken cancel);
    }
}
