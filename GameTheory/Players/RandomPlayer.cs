﻿// Copyright © 2016 John Gietzen.  All Rights Reserved.
// This source is subject to the MIT license.
// Please see license.md for more information.

namespace GameTheory.Players
{
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
        /// <param name="playerToken">The token that represents player.</param>
        public RandomPlayer(PlayerToken playerToken)
        {
            this.playerToken = playerToken;
        }

        /// <summary>
        /// Gets the <see cref="GameTheory.PlayerToken"/> that represents the player.
        /// </summary>
        public PlayerToken PlayerToken
        {
            get { return this.playerToken; }
        }

        /// <inheritdoc />
        public async Task<TMove> ChooseMove(IGameState<TMove> gameState, CancellationToken cancel)
        {
            await Task.Yield();

            var chosenMove = default(TMove);
            var count = 0;

            foreach (var move in gameState.GetAvailableMoves(this.playerToken))
            {
                var max = count + 1;

                if (GameTheory.Random.Instance.Next(max) == count)
                {
                    chosenMove = move;
                }

                count = max;
            }

            return chosenMove;
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}
