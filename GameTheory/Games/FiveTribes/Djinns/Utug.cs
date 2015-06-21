// -----------------------------------------------------------------------
// <copyright file="Utug.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes.Djinns
{
    using System.Collections.Generic;
    using System.Linq;
    using GameTheory.Games.FiveTribes.Moves;

    /// <summary>
    /// Pay <see cref="Cost.OneElderPlusOneElderOrOneSlave" /> to take control of 1 Tile with only Meeples on it (no Camel, Palm Tree or Palace); place 1 of your Camels on it.
    /// </summary>
    public class Utug : Djinn.PayPerActionDjinnBase
    {
        /// <summary>
        /// The singleton instance of <see cref="Utug" />.
        /// </summary>
        public static readonly Utug Instance = new Utug();

        private Utug()
            : base(4, Cost.OneElderPlusOneElderOrOneSlave)
        {
        }

        /// <inheritdoc />
        protected override bool CanGetMoves(GameState state)
        {
            return base.CanGetMoves(state) && state.IsPlayerUnderCamelLimit(state.ActivePlayer);
        }

        /// <inheritdoc />
        protected override IEnumerable<Move> GetAppliedCostMoves(GameState state0)
        {
            var meepleSquares = Enumerable.Range(0, Sultanate.Width * Sultanate.Height).Where(i => { var sq = state0.Sultanate[i]; return sq.Owner == null && sq.Meeples.Count >= 1 && sq.Palaces == 0 && sq.PalmTrees == 0; });

            return from i in meepleSquares
                   select new PlaceCamelMove(state0, i);
        }
    }
}
