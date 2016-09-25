// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Players
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Implements a player that consults a strategy before defering to another player.
    /// </summary>
    /// <typeparam name="TMove">The type of move that the player supports.</typeparam>
    public class StrategyPlayer<TMove> : IPlayer<TMove>
        where TMove : IMove
    {
        private readonly IPlayer<TMove> player;
        private readonly IStrategy<TMove> strategy;

        /// <summary>
        /// Initializes a new instance of the <see cref="StrategyPlayer{TMove}"/> class.
        /// </summary>
        /// <param name="player">The player to default to when the strategy doesn't yield a move.</param>
        /// <param name="strategy">The strategy to use before defering to the player.</param>
        public StrategyPlayer(IPlayer<TMove> player, IStrategy<TMove> strategy)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (strategy == null)
            {
                throw new ArgumentNullException(nameof(strategy));
            }

            this.player = player;
            this.strategy = strategy;
        }

        /// <inheritdoc/>
        public async Task<Maybe<TMove>> ChooseMove(IGameState<TMove> gameState, CancellationToken cancel)
        {
            var maybeMove = await this.strategy.ChooseMove(gameState, cancel);
            if (maybeMove.HasValue)
            {
                return maybeMove;
            }

            cancel.ThrowIfCancellationRequested();

            return await this.player.ChooseMove(gameState, cancel);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.player.Dispose();
            this.strategy.Dispose();
        }
    }
}
