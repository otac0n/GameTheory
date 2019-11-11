// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides utilities for working with <see cref="IGameState{TMove}"/> based games.
    /// </summary>
    public static class GameUtilities
    {
        /// <summary>
        /// Finds the next player in the specified game state's players collection.
        /// </summary>
        /// <typeparam name="TGameState">The type of game state that will be searched.</typeparam>
        /// <typeparam name="TMove">The type of moves in the game state.</typeparam>
        /// <param name="state">The game state to search.</param>
        /// <param name="currentPlayer">The current player.</param>
        /// <returns>The next player, or the first player if the current player was not found.</returns>
        public static PlayerToken GetNextPlayer<TGameState, TMove>(this TGameState state, PlayerToken currentPlayer)
            where TGameState : IGameState<TMove>
            where TMove : IMove
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            return state.Players.GetNextPlayer(currentPlayer);
        }

        /// <summary>
        /// Finds the next player in the specified list of players.
        /// </summary>
        /// <param name="players">The list of players to search.</param>
        /// <param name="currentPlayer">The current player.</param>
        /// <returns>The next player, or the first player if the current player was not found.</returns>
        public static PlayerToken GetNextPlayer(this IReadOnlyList<PlayerToken> players, PlayerToken currentPlayer)
        {
            if (players == null)
            {
                throw new ArgumentNullException(nameof(players));
            }

            return players[(players.IndexOf(currentPlayer) + 1) % players.Count];
        }

        /// <summary>
        /// Finds the next player in the specified list of players.
        /// </summary>
        /// <param name="players">The list of players to search.</param>
        /// <param name="currentPlayer">The current player.</param>
        /// <returns>The next player, or the first player if the current player was not found.</returns>
        public static PlayerToken GetNextPlayer(this IList<PlayerToken> players, PlayerToken currentPlayer)
        {
            if (players == null)
            {
                throw new ArgumentNullException(nameof(players));
            }

            return players[(players.IndexOf(currentPlayer) + 1) % players.Count];
        }

        /// <summary>
        /// Finds the next player in the specified list of players.
        /// </summary>
        /// <param name="players">The list of players to search.</param>
        /// <param name="currentPlayer">The current player.</param>
        /// <returns>The next player, or the first player if the current player was not found.</returns>
        public static PlayerToken GetNextPlayer(this ImmutableList<PlayerToken> players, PlayerToken currentPlayer) => ((IList<PlayerToken>)players).GetNextPlayer(currentPlayer);

        /// <summary>
        /// Finds the next player in the specified list of players.
        /// </summary>
        /// <param name="players">The list of players to search.</param>
        /// <param name="currentPlayer">The current player.</param>
        /// <returns>The next player, or the first player if the current player was not found.</returns>
        public static PlayerToken GetNextPlayer(this ImmutableArray<PlayerToken> players, PlayerToken currentPlayer) => ((IList<PlayerToken>)players).GetNextPlayer(currentPlayer);

        /// <summary>
        /// Gets a player number for display.
        /// </summary>
        /// <typeparam name="TMove">The type of object that represents a move in the game state.</typeparam>
        /// <param name="state">The game state.</param>
        /// <param name="playerToken">The player to search for.</param>
        /// <returns>A number representing the specified player token.</returns>
        public static int GetPlayerNumber<TMove>(this IGameState<TMove> state, PlayerToken playerToken)
            where TMove : IMove
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            var ix = state.Players.IndexOf(playerToken);
            return ix > -1 ? (ix + 1) : ix;
        }

        /// <summary>
        /// Plays the game from the specified game state as a task. The result of the task is the final game state.
        /// </summary>
        /// <typeparam name="TGameState">The type of game states that will be played through.</typeparam>
        /// <typeparam name="TMove">The type of object that represents a move in the game state.</typeparam>
        /// <param name="state">The starting game state.</param>
        /// <param name="getPlayer">A function that provides the player for each player token in the game state.</param>
        /// <param name="moveChosen">An action that is executed whenever a move is chosen.</param>
        /// <param name="timePerMove">The allowed time per move.</param>
        /// <returns>A task representing the ongoning operation.</returns>
        public static Task<TGameState> PlayGame<TGameState, TMove>(TGameState state, Func<PlayerToken, IPlayer<TGameState, TMove>> getPlayer, Action<TGameState, TMove, TGameState> moveChosen = null, TimeSpan? timePerMove = null)
            where TGameState : IGameState<TMove>
            where TMove : IMove
        {
            return PlayGame(state, playerTokens => playerTokens.Select(p => getPlayer(p)).ToArray(), moveChosen, timePerMove);
        }

        /// <summary>
        /// Plays the game from the specified game state as a task. The result of the task is the final game state.
        /// </summary>
        /// <typeparam name="TGameState">The type of game states that will be played through.</typeparam>
        /// <typeparam name="TMove">The type of object that represents a move in the game state.</typeparam>
        /// <param name="state">The starting game state.</param>
        /// <param name="getPlayers">A function that provides the list of players when given the list of player tokens in the game state.</param>
        /// <param name="moveChosen">An action that is executed whenever a move is chosen.</param>
        /// <param name="timePerMove">The allowed time per move.</param>
        /// <returns>A task representing the ongoning operation.</returns>
        public static async Task<TGameState> PlayGame<TGameState, TMove>(TGameState state, Func<IReadOnlyList<PlayerToken>, IReadOnlyList<IPlayer<TGameState, TMove>>> getPlayers, Action<TGameState, TMove, TGameState> moveChosen = null, TimeSpan? timePerMove = null)
            where TGameState : IGameState<TMove>
            where TMove : IMove
        {
            var playerTokens = state.Players;
            var players = getPlayers(playerTokens);
            try
            {
                var getTasks = new Func<CancellationToken, Task<Maybe<TMove>>[]>(cancel =>
                {
                    return players.Select(async p =>
                    {
                        var move = await p.ChooseMove((TGameState)state.GetView(p.PlayerToken, maxStates: 1).First(), cancel);
                        return move.HasValue && move.Value.PlayerToken == p.PlayerToken ? move : default;
                    }).ToArray();
                });

                while (true)
                {
                    var cts = new CancellationTokenSource();
                    cts.CancelAfter(timePerMove ?? TimeSpan.FromMinutes(1));
                    var tasks = new HashSet<Task<Maybe<TMove>>>(getTasks(cts.Token));

                    var chosenMove = default(Maybe<TMove>);

                    while (tasks.Count > 0)
                    {
                        var finishedTask = await Task.WhenAny(tasks);
                        tasks.Remove(finishedTask);

                        if (!(cts.IsCancellationRequested || finishedTask.IsFaulted))
                        {
                            chosenMove = finishedTask.Result;

                            if (chosenMove.HasValue)
                            {
                                break;
                            }
                        }
                    }

                    if (tasks.Count > 0)
                    {
                        // Drain remaing tasks to avoid concurrency issues.
                        cts.Cancel();

                        try
                        {
                            Task.WaitAll(tasks.ToArray());
                        }
                        catch (AggregateException aggEx)
                        {
                            aggEx.Handle(ex => ex is OperationCanceledException);
                        }
                    }

                    if (!chosenMove.HasValue)
                    {
                        break;
                    }

                    var before = state;
                    state = (TGameState)state.MakeMove(chosenMove.Value);
                    moveChosen?.Invoke(before, chosenMove.Value, state);
                }

                return state;
            }
            finally
            {
                for (var i = 0; i < players.Count; i++)
                {
                    players[i].Dispose();
                }
            }
        }
    }
}
