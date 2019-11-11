// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using GameTheory.ConsoleRunner.Properties;
    using GameTheory.ConsoleRunner.Shared;

    /// <summary>
    /// Implements a player who interacts with the game state via the processes console.
    /// </summary>
    /// <typeparam name="TGameState">The type of game state that will be played.</typeparam>
    /// <typeparam name="TMove">The type of moves that will be played.</typeparam>
    public sealed class ConsolePlayer<TGameState, TMove> : IPlayer<TGameState, TMove>
        where TGameState : IGameState<TMove>
        where TMove : IMove
    {
        private readonly IConsoleRenderer<TGameState, TMove> renderer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsolePlayer{TGameState, TMove}"/> class.
        /// </summary>
        /// <param name="playerToken">The player token that represents the player.</param>
        /// <param name="renderer">The console renderer to use.</param>
        public ConsolePlayer(PlayerToken playerToken, IConsoleRenderer<TGameState, TMove> renderer)
        {
            this.PlayerToken = playerToken;
            this.renderer = renderer;
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
            var playerColor = Shared.ConsoleInteraction.GetPlayerColor<TGameState, TMove>(state, this.PlayerToken);

            await Task.Yield();

            var moves = state.GetAvailableMoves<TGameState, TMove>(this.PlayerToken);
            if (moves.Any())
            {
                return ConsoleInteraction.WithLock(() =>
                {
                    Shared.ConsoleInteraction.WithColor(playerColor, () =>
                    {
                        Console.WriteLine(Resources.CurrentState);
                    });

                    Console.WriteLine();
                    this.renderer.Show(state, this.PlayerToken);
                    Console.WriteLine();

                    var result = default(Maybe<TMove>);
                    var originalColor = Console.ForegroundColor;

                    Shared.ConsoleInteraction.WithColor(playerColor, () =>
                    {
                        cancel.ThrowIfCancellationRequested();
                        result = ConsoleInteraction.Choose(moves.ToArray(), cancel, m =>
                        {
                            Shared.ConsoleInteraction.WithColor(originalColor, () =>
                            {
                                this.renderer.Show(state, m);
                            });
                        });
                    });
                    Console.WriteLine();
                    return result;
                });
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
