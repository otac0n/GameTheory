// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Tiles
{
    using System.Collections.Generic;
    using GameTheory.Games.FiveTribes.Moves;

    /// <summary>
    /// Represents a Village tile.
    /// </summary>
    public class Village : Tile
    {
        /// <summary>
        /// The singleton instance of <see cref="Village"/>.
        /// </summary>
        public static readonly Village Instance = new Village();

        private Village()
            : base(5, TileColor.Blue)
        {
        }

        /// <inheritdoc />
        public override IEnumerable<Move> GetTileActionMoves(GameState state)
        {
            yield return new PlacePalaceMove(state, state.LastPoint, s1 => s1.With(phase: Phase.MerchandiseSale));
        }
    }
}
