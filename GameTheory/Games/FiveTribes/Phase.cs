// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes
{
    /// <summary>
    /// Marks the current phase of a <see cref="GameState"/>.
    /// </summary>
    public enum Phase : byte
    {
        /// <summary>
        /// Bid for Turn order, phase 1.
        /// </summary>
        Bid,

        /// <summary>
        /// Move your Turn marker, phase 2.1.
        /// </summary>
        MoveTurnMarker,

        /// <summary>
        /// Pick up <see cref="Meeple">Meeples</see>, phase 2.2.
        /// </summary>
        PickUpMeeples,

        /// <summary>
        /// Move <see cref="Meeple">Meeples</see>, phase 2.2.
        /// </summary>
        MoveMeeples,

        /// <summary>
        /// Check for <see cref="Tile"/> control, phase 2.3.
        /// </summary>
        TileControlCheck,

        /// <summary>
        /// Do the <see cref="Meeple">Tribe's</see> Actions, phase 2.4.
        /// </summary>
        TribesAction,

        /// <summary>
        /// Do the <see cref="Tile">Tile's</see> Actions, phase 2.5.
        /// </summary>
        TileAction,

        /// <summary>
        /// Merchandise Sale (Optional) and Clean Up, Phase 2.6 and 3.
        /// </summary>
        MerchandiseSale,

        /// <summary>
        /// End of the game.
        /// </summary>
        End,
    }
}
