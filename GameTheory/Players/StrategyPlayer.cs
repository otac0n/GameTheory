// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

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
        private readonly IPlayer<TMove> fallbackPlayer;
        private readonly IStrategy<TMove> strategy;

        /// <summary>
        /// Initializes a new instance of the <see cref="StrategyPlayer{TMove}"/> class.
        /// </summary>
        /// <param name="strategy">The strategy to use before defering to the player.</param>
        /// <param name="fallbackPlayer">The player to default to when the strategy doesn't yield a move.</param>
        public StrategyPlayer(IStrategy<TMove> strategy, IPlayer<TMove> fallbackPlayer)
        {
            this.fallbackPlayer = fallbackPlayer ?? throw new ArgumentNullException(nameof(fallbackPlayer));
            this.strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="StrategyPlayer{TMove}"/> class.
        /// </summary>
        ~StrategyPlayer()
        {
            this.Dispose(false);
        }

        /// <inheritdoc />
        public event EventHandler<MessageSentEventArgs> MessageSent
        {
            add { }
            remove { }
        }

        /// <inheritdoc/>
        public PlayerToken PlayerToken => this.fallbackPlayer.PlayerToken;

        /// <inheritdoc/>
        public async Task<Maybe<TMove>> ChooseMove(IGameState<TMove> state, CancellationToken cancel)
        {
            var maybeMove = await this.strategy.ChooseMove(state, this.fallbackPlayer.PlayerToken, cancel);
            if (maybeMove.HasValue)
            {
                return maybeMove;
            }

            cancel.ThrowIfCancellationRequested();

            return await this.fallbackPlayer.ChooseMove(state, cancel);
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
            if (disposing)
            {
                this.fallbackPlayer.Dispose();
                this.strategy.Dispose();
            }
        }
    }
}
