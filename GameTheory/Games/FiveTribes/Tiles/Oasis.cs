// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Tiles
{
    using System.Collections.Generic;
    using GameTheory.Games.FiveTribes.Moves;

    /// <summary>
    /// Represents an Oasis tile.
    /// </summary>
    public class Oasis : Tile
    {
        /// <summary>
        /// The singleton instance of <see cref="Oasis"/>.
        /// </summary>
        public static readonly Oasis Instance = new Oasis();

        private Oasis()
            : base(8, TileColor.Red)
        {
        }

        /// <inheritdoc />
        public override IEnumerable<Move> GetTileActionMoves(GameState state)
        {
            yield return new PlacePalmTreeMove(state, state.LastPoint, s1 => s1.With(phase: Phase.MerchandiseSale));
        }
    }
}
