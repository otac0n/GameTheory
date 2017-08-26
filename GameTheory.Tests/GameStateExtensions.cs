// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Tests
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using GameTheory;

    internal static class GameStateExtensions
    {
        public static IGameState<TMove> PlayMove<TMove>(this IGameState<TMove> state, PlayerToken playerToken, Expression<Func<TMove, bool>> filter)
            where TMove : IMove
        {
            var moves = state.GetAvailableMoves(playerToken).Where(filter.Compile()).ToList();
            if (moves.Count != 1)
            {
                state.ShowMoves();
                Console.WriteLine("Filter: {0}", filter);
            }

            return state.MakeMove(moves.Single());
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
    }
}
