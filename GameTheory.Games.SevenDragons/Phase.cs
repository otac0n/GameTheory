// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.SevenDragons
{
    /// <summary>
    /// Represents the current phase the game.
    /// </summary>
    public enum Phase : byte
    {
        /// <summary>
        /// Draw a card.
        /// </summary>
        Draw,

        /// <summary>
        /// Play a card.
        /// </summary>
        Play,

        /// <summary>
        /// End of the game.
        /// </summary>
        End,
    }
}
