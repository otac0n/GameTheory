// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Players
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Implements a player that chooses randomly from its options.
    /// </summary>
    /// <typeparam name="TGameState">The type of game states that the player will evaluate.</typeparam>
    /// <typeparam name="TMove">The type of moves that the player will choose.</typeparam>
    public sealed class RandomPlayer<TGameState, TMove> : IPlayer<TGameState, TMove>
        where TGameState : IGameState<TMove>
        where TMove : IMove
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RandomPlayer{TGameState, TMove}"/> class.
        /// </summary>
        /// <param name="playerToken">The token that represents the player.</param>
        public RandomPlayer(PlayerToken playerToken)
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

            var chosenMove = default(TMove);
            var count = 0;

            foreach (var move in state.GetAvailableMoves<TGameState, TMove>(this.PlayerToken))
            {
                var max = count + 1;

                if (GameTheory.Random.Instance.Next(max) == count)
                {
                    chosenMove = move;
                }

                count = max;
            }

            return count == 0 ? default(Maybe<TMove>) : chosenMove;
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}
