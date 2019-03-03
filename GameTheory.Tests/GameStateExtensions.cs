// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using GameTheory;

    internal static class GameStateExtensions
    {
        public static IGameState<TMove> PlayAnyMove<TMove>(this IGameState<TMove> state, PlayerToken playerToken, Func<TMove, bool> filter, System.Random instance = null)
            where TMove : IMove
        {
            var moves = state.GetAvailableMoves(playerToken).Where(filter).ToList();
            if (moves.Count == 0)
            {
                state.ShowMoves();
            }

            var move = moves.Pick(instance);
            Console.WriteLine("Playing move:");
            Console.WriteLine($"{state.GetPlayerName(move.PlayerToken)}: {move}");
            return state.MakeMove(move);
        }

        public static IGameState<TMove> PlayMove<TMove>(this IGameState<TMove> state, PlayerToken playerToken, Expression<Func<TMove, bool>> filter)
            where TMove : IMove
        {
            var moves = state.GetAvailableMoves(playerToken).Where(filter.Compile()).ToList();
            if (moves.Count != 1)
            {
                state.ShowMoves();
                if (moves.Count > 1)
                {
                    state.ShowMoves(moves);
                }

                Console.WriteLine("Filter: {0}", filter);
            }

            var move = moves.Single();
            Console.WriteLine("Playing move:");
            Console.WriteLine($"{state.GetPlayerName(move.PlayerToken)}: {move}");
            return state.MakeMove(move);
        }

        public static void ShowMoves<TMove>(this IGameState<TMove> state)
            where TMove : IMove
        {
            Console.WriteLine("Available Moves:");
            foreach (var move in state.GetAvailableMoves())
            {
                Console.WriteLine($"{state.GetPlayerName(move.PlayerToken)}: {move}");
            }
        }

        public static void ShowMoves<TMove>(this IGameState<TMove> state, IEnumerable<TMove> moves)
            where TMove : IMove
        {
            Console.WriteLine("Moves:");
            foreach (var move in moves)
            {
                Console.WriteLine($"{state.GetPlayerName(move.PlayerToken)}: {move}");
            }
        }
    }
}
