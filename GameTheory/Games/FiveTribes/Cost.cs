namespace GameTheory.Games.FiveTribes
{
    using System;
    using System.Collections.Generic;
    using GameTheory.Games.FiveTribes.Moves;

    public delegate IEnumerable<Move> CostDelegate(GameState state, Func<GameState, GameState> after, Func<GameState, IEnumerable<Move>> getMoves);

    public static class Cost
    {
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

        public static IEnumerable<Move> OneElderOrOneSlave(GameState state0, Func<GameState, GameState> after, Func<GameState, IEnumerable<Move>> getMovesAfterCost)
        {
            var inventory0 = state0.Inventory[state0.ActivePlayer];

            if (inventory0.Meeples[Meeple.Elder] >= 1)
            {
                var move = new PayMeeplesMove(state0, Meeple.Elder, s1 => after(s1).WithMoves(getMovesAfterCost));

                if (state0.MakeMove(move).HasSubsequentMoves)
                {
                    yield return move;
                }
            }

            if (inventory0.Resources[Resource.Slave] >= 1)
            {
                var move = new PayResourcesMove(state0, Resource.Slave, s1 => after(s1).WithMoves(getMovesAfterCost));

                if (state0.MakeMove(move).HasSubsequentMoves)
                {
                    yield return move;
                }
            }
        }

        public static IEnumerable<Move> OneElderPlusOneElderOrOneSlave(GameState state0, Func<GameState, GameState> after, Func<GameState, IEnumerable<Move>> getMovesAfterCost)
        {
            var inventory0 = state0.Inventory[state0.ActivePlayer];

            if (inventory0.Meeples[Meeple.Elder] >= 2)
            {
                var move = new PayMeeplesMove(state0, new EnumCollection<Meeple>(Meeple.Elder, Meeple.Elder), s1 => after(s1).WithMoves(getMovesAfterCost));

                if (state0.MakeMove(move).HasSubsequentMoves)
                {
                    yield return move;
                }
            }

            if (inventory0.Meeples[Meeple.Elder] >= 1 && inventory0.Resources[Resource.Slave] >= 1)
            {
                var move = new PayMeeplesAndResourcesMove(state0, Meeple.Elder, Resource.Slave, s1 => after(s1).WithMoves(getMovesAfterCost));

                if (state0.MakeMove(move).HasSubsequentMoves)
                {
                    yield return move;
                }
            }
        }

        public static IEnumerable<Move> OneOrMoreSlaves(GameState state0, Func<GameState, GameState> after, Func<GameState, int, IEnumerable<Move>> getMovesAfterCost)
        {
            var slaves = state0.Inventory[state0.ActivePlayer].Resources[Resource.Slave];
            for (var i = 1; i <= slaves; i++)
            {
                var count = i;
                var move = new PayResourcesMove(state0, EnumCollection<Resource>.Empty.Add(Resource.Slave, count), s1 => after(s1).WithMoves(s2 => getMovesAfterCost(s2, count)));

                if (state0.MakeMove(move).HasSubsequentMoves)
                {
                    yield return move;
                }
            }
        }

        public static IEnumerable<Move> OneSlave(GameState state0, Func<GameState, GameState> after, Func<GameState, IEnumerable<Move>> getMovesAfterCost)
        {
            if (state0.Inventory[state0.ActivePlayer].Resources[Resource.Slave] >= 1)
            {
                var move = new PayResourcesMove(state0, Resource.Slave, s1 => after(s1).WithMoves(getMovesAfterCost));

                if (state0.MakeMove(move).HasSubsequentMoves)
                {
                    yield return move;
                }
            }
        }
    }
}
