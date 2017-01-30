﻿// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Extensions for all implementations of <see cref="IStrategy{TMove}"/>.
    /// </summary>
    public static class StrategyExtensions
    {
        /// <summary>
        /// Instructs the strategy to choose a move from the specified game state as an asynchronous operation using a task.
        /// </summary>
        /// <param name="strategy">The strategy to use.</param>
        /// <param name="gameState">The <see cref="IGameState{TMove}"/> for which the player will choose a move.</param>
        /// <param name="playerToken">The player who is considered to be using this strategy.</param>
        /// <param name="cancel">A <see cref="CancellationToken"/> that notifies a player if the request for a move is cancelled.</param>
        /// <returns>A task representing the ongoing operation.</returns>
        /// <typeparam name="TMove">The type of the moves that the strategy will choose.</typeparam>
        public static Task<Maybe<TMove>> ChooseMove<TMove>(this IStrategy<TMove> strategy, IGameState<TMove> gameState, PlayerToken playerToken, CancellationToken cancel)
            where TMove : IMove
        {
            var moves = gameState.GetAvailableMoves(playerToken);
            return strategy.ChooseMove(gameState, playerToken, moves, cancel);
        }
    }
}