// Copyright © 2016 John Gietzen.  All Rights Reserved.
// This source is subject to the MIT license.
// Please see license.md for more information.

namespace GameTheory.Strategies
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Implements a strategy that does not choose any move.
    /// </summary>
    /// <typeparam name="TMove">The type of the moves that the strategy will refuse to choose.</typeparam>
    public class NullStrategy<TMove> : IStrategy<TMove>
        where TMove : IMove
    {
        /// <inheritdoc/>
        public async Task<Maybe<TMove>> ChooseMove(IGameState<TMove> gameState, CancellationToken cancel)
        {
            await Task.Yield();

            return default(Maybe<TMove>);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}
