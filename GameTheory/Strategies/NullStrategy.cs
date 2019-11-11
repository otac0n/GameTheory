// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Strategies
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Implements a strategy that does not choose any move.
    /// </summary>
    /// <typeparam name="TGameState">The type of game states that the strategy will evaluate.</typeparam>
    /// <typeparam name="TMove">The type of the moves that the strategy will refuse to choose.</typeparam>
    public class NullStrategy<TGameState, TMove> : IStrategy<TGameState, TMove>
        where TGameState : IGameState<TMove>
        where TMove : IMove
    {
        /// <summary>
        /// Finalizes an instance of the <see cref="NullStrategy{TGameState, TMove}"/> class.
        /// </summary>
        ~NullStrategy()
        {
            this.Dispose(false);
        }

        /// <inheritdoc/>
        public async Task<Maybe<TMove>> ChooseMove(TGameState state, PlayerToken playerToken, IReadOnlyCollection<TMove> moves, CancellationToken cancel)
        {
            await Task.Yield();

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
