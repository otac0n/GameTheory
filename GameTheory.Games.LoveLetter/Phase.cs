// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.LoveLetter
{
    /// <summary>
    /// Represents the current phase of the game.
    /// </summary>
    public enum Phase : byte
    {
        /// <summary>
        /// The active player must draw a card.
        /// </summary>
        Draw,

        /// <summary>
        /// The active player must discard a card.
        /// </summary>
        Discard,

        /// <summary>
        /// The game must be redealt as the deck is empty.
        /// </summary>
        Deal,

        /// <summary>
        /// Players must reveal their hands.
        /// </summary>
        Reveal,

        /// <summary>
        /// End of the game.
        /// </summary>
        End,
    }
}
