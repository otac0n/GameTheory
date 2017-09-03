// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes
{
    using System;
    using System.Collections.Generic;
    using GameTheory.Games.FiveTribes.Moves;

    /// <summary>
    /// A delegate representing a standard cost.
    /// </summary>
    /// <param name="state">The initial state.</param>
    /// <param name="after">A function that should apply any necessary changes immediately after the cost is applied.</param>
    /// <returns>The cost's available moves.  This sequence will be empty if the active player cannot afford the cost, or if the cost results in no subsequent moves.</returns>
    public delegate IEnumerable<Move> ApplyCost(GameState state, Func<GameState, GameState> after);

    /// <summary>
    /// Provides utility methods for working with various costs in Five Tribes.
    /// </summary>
    public static class Cost
    {
        /// <summary>
        /// A method representing a cost in Gold.
        /// </summary>
        /// <param name="state">The initial state.</param>
        /// <param name="goldAmount">The amount of gold that will be paid.</param>
        /// <param name="after">A function that should apply any necessary changes immediately after the cost is applied.</param>
        /// <returns>The cost's available moves.  This sequence will be empty if the active player cannot afford the cost, or if the cost results in no subsequent moves.</returns>
        public static IEnumerable<Move> Gold(GameState state, int goldAmount, Func<GameState, GameState> after)
        {
            if (state.Inventory[state.ActivePlayer].GoldCoins >= goldAmount)
            {
                var move = new PayGoldMove(state, goldAmount, after);

                if (state.MakeMove(move).HasSubsequentMoves)
                {
                    yield return move;
                }
            }
        }

        /// <summary>
        /// A method representing a of one Elder or one Slave.
        /// </summary>
        /// <param name="state">The initial state.</param>
        /// <param name="after">A function that should apply any necessary changes immediately after the cost is applied.</param>
        /// <returns>The cost's available moves.  This sequence will be empty if the active player cannot afford the cost, or if the cost results in no subsequent moves.</returns>
        public static IEnumerable<Move> OneElderOrOneSlave(GameState state, Func<GameState, GameState> after)
        {
            var inventory0 = state.Inventory[state.ActivePlayer];

            if (inventory0.Meeples[Meeple.Elder] >= 1)
            {
                var move = new PayMeeplesMove(state, Meeple.Elder, after);

                if (state.MakeMove(move).HasSubsequentMoves)
                {
                    yield return move;
                }
            }

            if (inventory0.Resources[Resource.Slave] >= 1)
            {
                var move = new PayResourcesMove(state, Resource.Slave, after);

                if (state.MakeMove(move).HasSubsequentMoves)
                {
                    yield return move;
                }
            }
        }

        /// <summary>
        /// A method representing a cost of one Elder plus either one more Elder or a Slave.
        /// </summary>
        /// <param name="state">The initial state.</param>
        /// <param name="after">A function that should apply any necessary changes immediately after the cost is applied.</param>
        /// <returns>The cost's available moves.  This sequence will be empty if the active player cannot afford the cost, or if the cost results in no subsequent moves.</returns>
        public static IEnumerable<Move> OneElderPlusOneElderOrOneSlave(GameState state, Func<GameState, GameState> after)
        {
            var inventory0 = state.Inventory[state.ActivePlayer];

            if (inventory0.Meeples[Meeple.Elder] >= 2)
            {
                var move = new PayMeeplesMove(state, new EnumCollection<Meeple>(Meeple.Elder, Meeple.Elder), after);

                if (state.MakeMove(move).HasSubsequentMoves)
                {
                    yield return move;
                }
            }

            if (inventory0.Meeples[Meeple.Elder] >= 1 && inventory0.Resources[Resource.Slave] >= 1)
            {
                var move = new PayMeeplesAndResourcesMove(state, Meeple.Elder, Resource.Slave, after);

                if (state.MakeMove(move).HasSubsequentMoves)
                {
                    yield return move;
                }
            }
        }

        /// <summary>
        /// A method representing a cost of one or more Slaves.
        /// </summary>
        /// <param name="state">The initial state.</param>
        /// <param name="after">A function that should apply any necessary changes immediately after the cost is applied.</param>
        /// <returns>The cost's available moves.  This sequence will be empty if the active player cannot afford the cost, or if the cost results in no subsequent moves.</returns>
        public static IEnumerable<Move> OneOrMoreSlaves(GameState state, Func<GameState, int, GameState> after)
        {
            var slaves = state.Inventory[state.ActivePlayer].Resources[Resource.Slave];
            for (var count = 1; count <= slaves; count++)
            {
                var move = new PayResourcesMove(state, EnumCollection<Resource>.Empty.Add(Resource.Slave, count), s1 => after(s1, count));

                if (state.MakeMove(move).HasSubsequentMoves)
                {
                    yield return move;
                }
            }
        }

        /// <summary>
        /// A method representing a cost of one Slave.
        /// </summary>
        /// <param name="state">The initial state.</param>
        /// <param name="after">A function that should apply any necessary changes immediately after the cost is applied.</param>
        /// <returns>The cost's available moves.  This sequence will be empty if the active player cannot afford the cost, or if the cost results in no subsequent moves.</returns>
        public static IEnumerable<Move> OneSlave(GameState state, Func<GameState, GameState> after)
        {
            if (state.Inventory[state.ActivePlayer].Resources[Resource.Slave] >= 1)
            {
                var move = new PayResourcesMove(state, Resource.Slave, after);

                if (state.MakeMove(move).HasSubsequentMoves)
                {
                    yield return move;
                }
            }
        }
    }
}
