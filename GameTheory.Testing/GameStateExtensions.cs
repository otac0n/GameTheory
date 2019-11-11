// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using GameTheory;

    public static class GameStateExtensions
    {
        public static TGameState PlayAnyMove<TGameState, TMove>(this TGameState state, PlayerToken playerToken, Func<TMove, bool> filter, System.Random instance = null)
            where TGameState : IGameState<TMove>
            where TMove : IMove
        {
            var moves = state.GetAvailableMoves<TGameState, TMove>(playerToken).Where(filter).ToList();
            if (moves.Count == 0)
            {
                state.ShowMoves<TGameState, TMove>();
            }

            var move = moves.Pick(instance);
            Console.WriteLine("Playing move:");
            Console.WriteLine($"{state.GetPlayerName<TGameState, TMove>(move.PlayerToken)}: {move}");
            return (TGameState)state.MakeMove(move);
        }

        public static TGameState PlayMove<TGameState, TMove>(this TGameState state, PlayerToken playerToken, Expression<Func<TMove, bool>> filter)
            where TGameState : IGameState<TMove>
            where TMove : IMove
        {
            var moves = state.GetAvailableMoves<TGameState, TMove>(playerToken).Where(filter.Compile()).ToList();
            if (moves.Count != 1)
            {
                state.ShowMoves<TGameState, TMove>();
                if (moves.Count > 1)
                {
                    state.ShowMoves(moves);
                }

                Console.WriteLine("Filter: {0}", filter);
            }

            var move = moves.Single();
            Console.WriteLine("Playing move:");
            Console.WriteLine($"{state.GetPlayerName<TGameState, TMove>(move.PlayerToken)}: {move}");
            return (TGameState)state.MakeMove(move);
        }

        public static void ShowMoves<TGameState, TMove>(this TGameState state)
            where TGameState : IGameState<TMove>
            where TMove : IMove
        {
            Console.WriteLine("Available Moves:");
            foreach (var move in state.GetAvailableMoves())
            {
                Console.WriteLine($"{state.GetPlayerName<TGameState, TMove>(move.PlayerToken)}: {move}");
            }
        }

        public static void ShowMoves<TGameState, TMove>(this TGameState state, IEnumerable<TMove> moves)
            where TGameState : IGameState<TMove>
            where TMove : IMove
        {
            Console.WriteLine("Moves:");
            foreach (var move in moves)
            {
                Console.WriteLine($"{state.GetPlayerName<TGameState, TMove>(move.PlayerToken)}: {move}");
            }
        }
    }
}
