// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Strategies
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Implements a strategy that looks a limited depth for a win before giving up.
    /// </summary>
    /// <typeparam name="TMove">The type of the moves that the strategy will choose.</typeparam>
    public class ImmediateWinStrategy<TMove> : IStrategy<TMove>
        where TMove : IMove
    {
        /// <summary>
        /// Finalizes an instance of the <see cref="ImmediateWinStrategy{TMove}"/> class.
        /// </summary>
        ~ImmediateWinStrategy()
        {
            this.Dispose(false);
        }

        /// <inheritdoc/>
        public async Task<Maybe<TMove>> ChooseMove(IGameState<TMove> state, PlayerToken playerToken, IReadOnlyCollection<TMove> moves, CancellationToken cancel)
        {
            await Task.Yield();

            foreach (var move in moves.Where(m => m.PlayerToken == playerToken))
            {
                var nextState = state.MakeMove(move);
                if (nextState.GetWinners().Contains(playerToken))
                {
                    return move;
                }
            }

            return default(Maybe<TMove>);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the Component and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
