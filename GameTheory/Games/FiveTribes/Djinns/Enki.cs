// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Djinns
{
    using System.Collections.Generic;
    using System.Linq;
    using GameTheory.Games.FiveTribes.Moves;
    using GameTheory.Games.FiveTribes.Tiles;

    /// <summary>
    /// Pay <see cref="Cost.OneElderOrOneSlave"/> to place 1 Palm Tree on any Oasis.
    /// </summary>
    public class Enki : PayPerActionDjinnBase
    {
        /// <summary>
        /// The singleton instance of <see cref="Enki"/>.
        /// </summary>
        public static readonly Enki Instance = new Enki();

        private Enki()
            : base(8, Cost.OneElderOrOneSlave)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<Move> GetAppliedCostMoves(GameState state)
        {
            return Enumerable.Range(0, Sultanate.Width * Sultanate.Height)
                .Where(i => state.Sultanate[i].Tile is Oasis)
                .Select(i => new PlacePalmTreeMove(state, i));
        }
    }
}
