// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Strategies
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Implements a strategy that does not choose any move.
    /// </summary>
    /// <typeparam name="TGameState">The type of game states that the strategy will evaluate.</typeparam>
    /// <typeparam name="TMove">The type of the moves that the strategy will refuse to choose.</typeparam>
    public sealed class NullStrategy<TGameState, TMove> : IStrategy<TGameState, TMove>
        where TGameState : IGameState<TMove>
        where TMove : IMove
    {
        /// <inheritdoc/>
        public Task<Maybe<TMove>> ChooseMove(TGameState state, PlayerToken playerToken, IReadOnlyCollection<TMove> moves, CancellationToken cancel)
        {
            return Task.FromResult(default(Maybe<TMove>));
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}
