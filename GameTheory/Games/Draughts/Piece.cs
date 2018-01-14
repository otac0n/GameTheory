// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Draughts
{
    using System;

    /// <summary>
    /// Represents the state of a piece at a certain square.
    /// </summary>
    [Flags]
    public enum Piece : byte
    {
        /// <summary>
        /// No piece is present.
        /// </summary>
        None = 0x00,

        /// <summary>
        /// The piece present belongs to the white player.
        /// </summary>
        White = 0x01,

        /// <summary>
        /// The piece present belongs to the black player.
        /// </summary>
        Black = 0x02,

        /// <summary>
        /// The piece has been crowned.
        /// </summary>
        Crowned = 0x04,

        /// <summary>
        /// The piece has been captured but not yet removed.
        /// </summary>
        Captured = 0x08,
    }
}
