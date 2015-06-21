﻿// -----------------------------------------------------------------------
// <copyright file="Cost.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes
{
    using System;
    using System.Collections.Generic;
    using GameTheory.Games.FiveTribes.Moves;

    /// <summary>
    /// A delegate representing a standard cost.
    /// </summary>
    /// <param name="state0">The initial state.</param>
    /// <param name="after">A function that should apply any necessary changes immediately after the cost is applied.</param>
    /// <param name="getMoves">A function that should return the available moves after applying the cost and <paramref name="after"/> function.</param>
    /// <returns>The cost's available moves.  This sequence will be empty if the active player cannot afford the cost, or if the cost results in no subsequent moves.</returns>
    public delegate IEnumerable<Move> CostDelegate(GameState state0, Func<GameState, GameState> after, Func<GameState, IEnumerable<Move>> getMoves);

    public static class Cost
    {
        /// <summary>
        /// A method representing a cost in Gold.
        /// </summary>
        /// <param name="state0">The initial state.</param>
        /// <param name="gold">The amount of gold that will be paid.</param>
        /// <param name="after">A function that should apply any necessary changes immediately after the cost is applied.</param>
        /// <param name="getMoves">A function that should return the available moves after applying the cost and <paramref name="after"/> function.</param>
        /// <returns>The cost's available moves.  This sequence will be empty if the active player cannot afford the cost, or if the cost results in no subsequent moves.</returns>
        public static IEnumerable<Move> Gold(GameState state0, int gold, Func<GameState, GameState> after, Func<GameState, IEnumerable<Move>> getMovesAfterCost)
        {
            if (state0.Inventory[state0.ActivePlayer].GoldCoins >= gold)
            {
                var move = new PayGoldMove(state0, gold, s1 => after(s1).WithMoves(getMovesAfterCost));

                if (state0.MakeMove(move).HasSubsequentMoves)
                {
                    yield return move;
                }
            }
        }

        /// <summary>
        /// A method representing a of one Elder or one Slave.
        /// </summary>
        /// <param name="state0">The initial state.</param>
        /// <param name="after">A function that should apply any necessary changes immediately after the cost is applied.</param>
        /// <param name="getMoves">A function that should return the available moves after applying the cost and <paramref name="after"/> function.</param>
        /// <returns>The cost's available moves.  This sequence will be empty if the active player cannot afford the cost, or if the cost results in no subsequent moves.</returns>
        public static IEnumerable<Move> OneElderOrOneSlave(GameState state0, Func<GameState, GameState> after, Func<GameState, IEnumerable<Move>> getMoves)
        {
            var inventory0 = state0.Inventory[state0.ActivePlayer];

            if (inventory0.Meeples[Meeple.Elder] >= 1)
            {
                var move = new PayMeeplesMove(state0, Meeple.Elder, s1 => after(s1).WithMoves(getMoves));

                if (state0.MakeMove(move).HasSubsequentMoves)
                {
                    yield return move;
                }
            }

            if (inventory0.Resources[Resource.Slave] >= 1)
            {
                var move = new PayResourcesMove(state0, Resource.Slave, s1 => after(s1).WithMoves(getMoves));

                if (state0.MakeMove(move).HasSubsequentMoves)
                {
                    yield return move;
                }
            }
        }

        /// <summary>
        /// A method representing a cost of one Elder plus either one more Elder or a Slave.
        /// </summary>
        /// <param name="state0">The initial state.</param>
        /// <param name="after">A function that should apply any necessary changes immediately after the cost is applied.</param>
        /// <param name="getMoves">A function that should return the available moves after applying the cost and <paramref name="after"/> function.</param>
        /// <returns>The cost's available moves.  This sequence will be empty if the active player cannot afford the cost, or if the cost results in no subsequent moves.</returns>
        public static IEnumerable<Move> OneElderPlusOneElderOrOneSlave(GameState state0, Func<GameState, GameState> after, Func<GameState, IEnumerable<Move>> getMoves)
        {
            var inventory0 = state0.Inventory[state0.ActivePlayer];

            if (inventory0.Meeples[Meeple.Elder] >= 2)
            {
                var move = new PayMeeplesMove(state0, new EnumCollection<Meeple>(Meeple.Elder, Meeple.Elder), s1 => after(s1).WithMoves(getMoves));

                if (state0.MakeMove(move).HasSubsequentMoves)
                {
                    yield return move;
                }
            }

            if (inventory0.Meeples[Meeple.Elder] >= 1 && inventory0.Resources[Resource.Slave] >= 1)
            {
                var move = new PayMeeplesAndResourcesMove(state0, Meeple.Elder, Resource.Slave, s1 => after(s1).WithMoves(getMoves));

                if (state0.MakeMove(move).HasSubsequentMoves)
                {
                    yield return move;
                }
            }
        }

        /// <summary>
        /// A method representing a cost of one or more Slaves.
        /// </summary>
        /// <param name="state0">The initial state.</param>
        /// <param name="after">A function that should apply any necessary changes immediately after the cost is applied.</param>
        /// <param name="getMoves">A function that should return the available moves after applying the cost and <paramref name="after"/> function.</param>
        /// <returns>The cost's available moves.  This sequence will be empty if the active player cannot afford the cost, or if the cost results in no subsequent moves.</returns>
        public static IEnumerable<Move> OneOrMoreSlaves(GameState state0, Func<GameState, GameState> after, Func<GameState, int, IEnumerable<Move>> getMoves)
        {
            var slaves = state0.Inventory[state0.ActivePlayer].Resources[Resource.Slave];
            for (var i = 1; i <= slaves; i++)
            {
                var count = i;
                var move = new PayResourcesMove(state0, EnumCollection<Resource>.Empty.Add(Resource.Slave, count), s1 => after(s1).WithMoves(s2 => getMoves(s2, count)));

                if (state0.MakeMove(move).HasSubsequentMoves)
                {
                    yield return move;
                }
            }
        }

        /// <summary>
        /// A method representing a cost of one Slave.
        /// </summary>
        /// <param name="state0">The initial state.</param>
        /// <param name="after">A function that should apply any necessary changes immediately after the cost is applied.</param>
        /// <param name="getMoves">A function that should return the available moves after applying the cost and <paramref name="after"/> function.</param>
        /// <returns>The cost's available moves.  This sequence will be empty if the active player cannot afford the cost, or if the cost results in no subsequent moves.</returns>
        public static IEnumerable<Move> OneSlave(GameState state0, Func<GameState, GameState> after, Func<GameState, IEnumerable<Move>> getMoves)
        {
            if (state0.Inventory[state0.ActivePlayer].Resources[Resource.Slave] >= 1)
            {
                var move = new PayResourcesMove(state0, Resource.Slave, s1 => after(s1).WithMoves(getMoves));

                if (state0.MakeMove(move).HasSubsequentMoves)
                {
                    yield return move;
                }
            }
        }
    }
}