﻿namespace GameTheory.Games.FiveTribes.Djinns
{
    using System.Collections.Generic;
    using System.Linq;
    using GameTheory.Games.FiveTribes.Moves;

    public class Utug : Djinn.PayPerActionDjinnBase
    {
        public static readonly Utug Instance = new Utug();

        private Utug()
            : base(4, Cost.OneElderPlusOneElderOrOneSlave)
        {
        }

        protected override bool CanGetMoves(GameState state)
        {
            return base.CanGetMoves(state) && state.IsPlayerUnderCamelLimit(state.ActivePlayer);
        }

        protected override IEnumerable<Move> GetAppliedCostMoves(GameState state0)
        {
            var meepleSquares = Enumerable.Range(0, Sultanate.Width * Sultanate.Height).Where(i => { var sq = state0.Sultanate[i]; return sq.Owner == null && sq.Meeples.Count >= 1 && sq.Palaces == 0 && sq.PalmTrees == 0; });

            return from i in meepleSquares
                   select new PlaceCamelMove(state0, i);
        }
    }
}
