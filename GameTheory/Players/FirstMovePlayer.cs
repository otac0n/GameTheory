// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Players
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Implements a player that chooses the first move from its options.
    /// </summary>
    /// <typeparam name="TGameState">The type of game states that the player will evaluate.</typeparam>
    /// <typeparam name="TMove">The type of moves that the player will choose.</typeparam>
    public sealed class FirstMovePlayer<TGameState, TMove> : IPlayer<TGameState, TMove>
        where TGameState : IGameState<TMove>
        where TMove : IMove
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FirstMovePlayer{TGameState, TMove}"/> class.
        /// </summary>
        /// <param name="playerToken">The token that represents the player.</param>
        public FirstMovePlayer(PlayerToken playerToken)
        {
            this.PlayerToken = playerToken;
        }

        /// <inheritdoc />
        public event EventHandler<MessageSentEventArgs> MessageSent
        {
            add { }
            remove { }
        }

        /// <inheritdoc />
        public PlayerToken PlayerToken { get; }

        /// <inheritdoc />
        public async Task<Maybe<TMove>> ChooseMove(TGameState state, CancellationToken cancel)
        {
            await Task.Yield();
            return state
                .GetAvailableMoves<TGameState, TMove>(this.PlayerToken)
                .Select(m => new Maybe<TMove>(m))
                .FirstOrDefault();
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}
