// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.CenturySpiceRoad
{
    /// <summary>
    /// Represents the current phase of the game.
    /// </summary>
    public enum Phase : byte
    {
        /// <summary>
        /// The game is in play.
        /// </summary>
        Play,

        /// <summary>
        /// The active player must pay for or acquire a merchant card.
        /// </summary>
        Acquire,

        /// <summary>
        /// The active player has available upgrades to use.
        /// </summary>
        Upgrade,

        /// <summary>
        /// The active player must discard before play can continue.
        /// </summary>
        Discard,

        /// <summary>
        /// End of the game.
        /// </summary>
        End,
    }
}
