﻿// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Djinns
{
    using System.Collections.Generic;
    using System.Linq;
    using GameTheory.Games.FiveTribes.Moves;
    using GameTheory.Games.FiveTribes.Tiles;

    /// <summary>
    /// Pay <see cref="Cost.OneElderOrOneSlave"/> to place 1 Palace on any Village.
    /// </summary>
    public class Bouraq : PayPerActionDjinnBase
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
        protected override IEnumerable<Move> GetAppliedCostMoves(GameState state)
        {
            return Enumerable.Range(0, Sultanate.Width * Sultanate.Height)
                .Where(i => state.Sultanate[i].Tile is Village)
                .Select(i => new PlacePalaceMove(state, i));
        }
    }
}
