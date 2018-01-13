// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Players
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Implements a player that chooses randomly from its options.
    /// </summary>
    /// <typeparam name="TMove">The type of move that the player supports.</typeparam>
    public sealed class RandomPlayer<TMove> : IPlayer<TMove>
        where TMove : IMove
    {
        private readonly PlayerToken playerToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomPlayer{TMove}"/> class.
        /// </summary>
        /// <param name="playerToken">The token that represents the player.</param>
        public RandomPlayer(PlayerToken playerToken)
        {
            this.playerToken = playerToken;
        }

        /// <inheritdoc />
        public event EventHandler<MessageSentEventArgs> MessageSent
        {
            add { }
            remove { }
        }

        /// <inheritdoc />
        public PlayerToken PlayerToken => this.playerToken;

        /// <inheritdoc />
        public async Task<Maybe<TMove>> ChooseMove(IGameState<TMove> state, CancellationToken cancel)
        {
            await Task.Yield();

            var chosenMove = default(TMove);
            var count = 0;

            foreach (var move in state.GetAvailableMoves(this.playerToken))
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
