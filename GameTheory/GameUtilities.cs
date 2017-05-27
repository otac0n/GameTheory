﻿// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides utilities for working with <see cref="IGameState{TMove}"/> based games.
    /// </summary>
    public static class GameUtilities
    {
        /// <summary>
        /// Gets a player name for display.
        /// </summary>
        /// <typeparam name="TMove">The type of object that represents a move in the game state.</typeparam>
        /// <param name="state">The game state.</param>
        /// <param name="playerToken">The player to search for.</param>
        /// <returns>A name representing the specified player token.</returns>
        public static string GetPlayerName<TMove>(this IGameState<TMove> state, PlayerToken playerToken)
            where TMove : IMove
        {
            return $"Player {state.GetPlayerNumber(playerToken)}";
        }

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

            for (int i = 0; i < state.Players.Count; i++)
            {
                if (state.Players[i] == playerToken)
                {
                    return i + 1;
                }
            }

            return -1;
        }

        /// <summary>
        /// Plays the game from the specified game state as a task. The result of the task is the final game state.
        /// </summary>
        /// <typeparam name="TMove">The type of object that represents a move in the game state.</typeparam>
        /// <param name="state">The starting game state.</param>
        /// <param name="getPlayer">A function that provides the player for each player token in the game state.</param>
        /// <param name="moveChosen">An action that is executed whenever a move is chosen.</param>
        /// <param name="timePerMove">The allowed time per move.</param>
        /// <returns>A task representing the ongoning operation.</returns>
        public static Task<IGameState<TMove>> PlayGame<TMove>(IGameState<TMove> state, Func<PlayerToken, IPlayer<TMove>> getPlayer, Action<IGameState<TMove>, TMove> moveChosen = null, TimeSpan? timePerMove = null)
            where TMove : IMove
        {
            return PlayGame(state, playerTokens => playerTokens.Select(p => getPlayer(p)).ToArray(), moveChosen, timePerMove);
        }

        /// <summary>
        /// Plays the game from the specified game state as a task. The result of the task is the final game state.
        /// </summary>
        /// <typeparam name="TMove">The type of object that represents a move in the game state.</typeparam>
        /// <param name="state">The starting game state.</param>
        /// <param name="getPlayers">A function that provides the list of players when given the list of player tokens in the game state.</param>
        /// <param name="moveChosen">An action that is executed whenever a move is chosen.</param>
        /// <param name="timePerMove">The allowed time per move.</param>
        /// <returns>A task representing the ongoning operation.</returns>
        public static async Task<IGameState<TMove>> PlayGame<TMove>(IGameState<TMove> state, Func<IReadOnlyList<PlayerToken>, IReadOnlyList<IPlayer<TMove>>> getPlayers, Action<IGameState<TMove>, TMove> moveChosen = null, TimeSpan? timePerMove = null)
            where TMove : IMove
        {
            var playerTokens = state.Players;
            var players = getPlayers(playerTokens);
            try
            {
                var getTasks = new Func<CancellationToken, Task<Maybe<TMove>>[]>(cancel => players.Select(p => p.ChooseMove(state.GetView(p.PlayerToken), cancel)).ToArray());
                while (true)
                {
                    var cts = new CancellationTokenSource();
                    cts.CancelAfter(timePerMove ?? TimeSpan.FromMinutes(1));
                    var tasks = new HashSet<Task<Maybe<TMove>>>(getTasks(cts.Token));

                    Maybe<TMove> chosenMove = default(Maybe<TMove>);

                    while (tasks.Count > 0)
                    {
                        var finishedTask = await Task.WhenAny(tasks);
                        tasks.Remove(finishedTask);
                        if (finishedTask.Result.HasValue)
                        {
                            chosenMove = finishedTask.Result;
                            cts.Cancel();
                            break;
                        }
                    }

                    if (!chosenMove.HasValue)
                    {
                        break;
                    }

                    moveChosen?.Invoke(state, chosenMove.Value);

                    state = state.MakeMove(chosenMove.Value);
                }

                return state;
            }
            finally
            {
                for (int i = 0; i < players.Count; i++)
                {
                    players[i].Dispose();
                }
            }
        }
    }
}