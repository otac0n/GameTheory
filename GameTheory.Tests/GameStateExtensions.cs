// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Tests
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using GameTheory;

    internal static class GameStateExtensions
    {
        public static IGameState<TMove> PlayMove<TMove>(this IGameState<TMove> gameState, PlayerToken playerToken, Expression<Func<TMove, bool>> filter)
            where TMove : IMove
        {
            var moves = gameState.GetAvailableMoves(playerToken).Where(filter.Compile()).ToList();
            if (moves.Count != 1)
            {
                gameState.ShowMoves();
                Console.WriteLine("Filter: {0}", filter);
            }

            return gameState.MakeMove(moves.Single());
        }

        public static void ShowMoves<TMove>(this IGameState<TMove> state)
            where TMove : IMove
        {
            Console.WriteLine("Available Moves:");
            foreach (var move in state.GetAvailableMoves())
            {
                Console.WriteLine("{0}: {1} ", PlayerName(state, move.PlayerToken), move);
            }
        }

        public static string PlayerName<TMove>(this IGameState<TMove> state, PlayerToken playerToken)
            where TMove : IMove
        {
            return ((char)('A' + state.Players.ToList().IndexOf(playerToken))).ToString();
        }
    }
}
