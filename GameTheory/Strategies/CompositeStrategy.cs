// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Strategies
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Implements a strategy that composes together a collection of other strategies.
    /// </summary>
    /// <typeparam name="TGameState">The type of game states that the strategy will evaluate.</typeparam>
    /// <typeparam name="TMove">The type of the moves that the strategy will choose.</typeparam>
    public class CompositeStrategy<TGameState, TMove> : IStrategy<TGameState, TMove>
        where TGameState : IGameState<TMove>
        where TMove : IMove
    {
        private readonly bool disposeChildren;
        private readonly IReadOnlyCollection<IStrategy<TGameState, TMove>> strategies;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeStrategy{TMove}"/> class.
        /// </summary>
        /// <param name="strategies">The strategies that together make up this strategy.</param>
        public CompositeStrategy(params IStrategy<TGameState, TMove>[] strategies)
            : this(strategies, disposeChildren: false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeStrategy{TMove}"/> class.
        /// </summary>
        /// <param name="strategies">The strategies that together make up this strategy.</param>
        /// <param name="disposeChildren">A value indicating whether or not this strategy should own the disposal of the specified strategies.</param>
        public CompositeStrategy(IStrategy<TGameState, TMove>[] strategies, bool disposeChildren)
        {
            this.strategies = strategies.ToImmutableList();
            this.disposeChildren = disposeChildren;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="CompositeStrategy{TMove}"/> class.
        /// </summary>
        ~CompositeStrategy()
        {
            this.Dispose(false);
        }

        /// <inheritdoc/>
        public async Task<Maybe<TMove>> ChooseMove(TGameState state, PlayerToken playerToken, IReadOnlyCollection<TMove> moves, CancellationToken cancel)
        {
            foreach (var strategy in this.strategies)
            {
                var move = await strategy.ChooseMove(state, playerToken, moves, cancel);
                if (move.HasValue)
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
            if (disposing)
            {
                if (this.disposeChildren)
                {
                    foreach (var strategy in this.strategies)
                    {
                        strategy.Dispose();
                    }
                }
            }
        }
    }
}
