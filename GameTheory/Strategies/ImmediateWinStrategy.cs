// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Strategies
{
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
        private readonly PlayerToken player;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmediateWinStrategy{TMove}"/> class.
        /// </summary>
        /// <param name="player">A <see cref="PlayerToken"/> representing the player whose winning moves should be chosen.</param>
        public ImmediateWinStrategy(PlayerToken player)
        {
            this.player = player;
        }

        /// <inheritdoc/>
        public async Task<Maybe<TMove>> ChooseMove(IGameState<TMove> gameState, CancellationToken cancel)
        {
            await Task.Yield();

            var moves = gameState.GetAvailableMoves(this.player);
            foreach (var move in moves)
            {
                var nextState = gameState.MakeMove(move);
                if (nextState.GetWinners().Contains(this.player))
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
