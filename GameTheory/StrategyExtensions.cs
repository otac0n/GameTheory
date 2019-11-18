// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides extensions for all implementations of <see cref="IStrategy{TMove}"/>.
    /// </summary>
    public static class StrategyExtensions
    {
        /// <summary>
        /// Instructs the strategy to choose a move from the specified game state as an asynchronous operation using a task.
        /// </summary>
        /// <typeparam name="TGameState">The type of game state that will be evaluated.</typeparam>
        /// <typeparam name="TMove">The type of the moves that the strategy will choose.</typeparam>
        /// <param name="strategy">The strategy to use.</param>
        /// <param name="state">The <see cref="IGameState{TMove}"/> for which the player will choose a move.</param>
        /// <param name="playerToken">The player who is considered to be using this strategy.</param>
        /// <param name="cancel">A <see cref="CancellationToken"/> that notifies a player if the request for a move is cancelled.</param>
        /// <returns>A task representing the ongoing operation.</returns>
        public static Task<Maybe<TMove>> ChooseMove<TGameState, TMove>(this IStrategy<TGameState, TMove> strategy, TGameState state, PlayerToken playerToken, CancellationToken cancel)
            where TGameState : IGameState<TMove>
            where TMove : IMove
        {
            if (strategy == null)
            {
                throw new ArgumentNullException(nameof(strategy));
            }

            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            var moves = state.GetAvailableMoves<TGameState, TMove>(playerToken);
            return strategy.ChooseMove(state, playerToken, moves, cancel);
        }
    }
}
