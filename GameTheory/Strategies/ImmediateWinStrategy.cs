// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Strategies
{
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
        /// <inheritdoc/>
        public async Task<Maybe<TMove>> ChooseMove(IGameState<TMove> gameState, PlayerToken playerToken, IReadOnlyCollection<TMove> moves, CancellationToken cancel)
        {
            await Task.Yield();

            foreach (var move in moves.Where(m => m.PlayerToken == playerToken))
            {
                var nextState = gameState.MakeMove(move);
                if (nextState.GetWinners().Contains(playerToken))
                {
                    return new Maybe<TMove>(move);
                }
            }

            return default(Maybe<TMove>);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}
