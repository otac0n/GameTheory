// Copyright © 2016 John Gietzen.  All Rights Reserved.
// This source is subject to the MIT license.
// Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Djinns
{
    using System.Collections.Generic;
    using System.Linq;
    using GameTheory.Games.FiveTribes.Moves;

    /// <summary>
    /// Pay <see cref="Cost.OneElderOrOneSlave"/> to place 1 Palace on any Village.
    /// </summary>
    public class Bouraq : Djinn.PayPerActionDjinnBase
    {
        /// <summary>
        /// The singleton instance of <see cref="Bouraq"/>.
        /// </summary>
        public static readonly Bouraq Instance = new Bouraq();

        private Bouraq()
            : base(6, Cost.OneElderOrOneSlave)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<Move> GetAppliedCostMoves(GameState state0)
        {
            return Enumerable.Range(0, Sultanate.Width * Sultanate.Height)
                .Where(i => state0.Sultanate[i].Tile is Tile.Village)
                .Select(i => new PlacePalaceMove(state0, i));
        }
    }
}
