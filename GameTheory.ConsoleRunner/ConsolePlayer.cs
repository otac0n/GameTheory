﻿// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using GameTheory.ConsoleRunner.Properties;

    /// <summary>
    /// Implements a player who interacts with the game state via the processes console.
    /// </summary>
    /// <typeparam name="TMove">The type of moves that will be played.</typeparam>
    public sealed class ConsolePlayer<TMove> : IPlayer<TMove>
        where TMove : IMove
    {
        private static readonly object Sync = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsolePlayer{TMove}"/> class.
        /// </summary>
        /// <param name="playerToken">The player token that represents the player.</param>
        public ConsolePlayer(PlayerToken playerToken)
        {
            this.PlayerToken = playerToken;
        }

        /// <inheritdoc />
        public PlayerToken PlayerToken { get; }

        /// <inheritdoc />
        public async Task<Maybe<TMove>> ChooseMove(IGameState<TMove> state, CancellationToken cancel)
        {
            await Task.Yield();

            var moves = state.GetAvailableMoves(this.PlayerToken);
            if (moves.Any())
            {
                lock (Sync)
                {
                    cancel.ThrowIfCancellationRequested();
                    Console.WriteLine(Resources.CurrentState);
                    Console.WriteLine(state);
                    return new Maybe<TMove>(ConsoleInteraction.Choose(moves.ToArray(), cancel));
                }
            }
            else
            {
                return default(Maybe<TMove>);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}
