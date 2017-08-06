// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Splendor
{
    /// <summary>
    /// The current phase of the game.
    /// </summary>
    public enum Phase
    {
        /// <summary>
        /// The game is in play.
        /// </summary>
        Play,

        /// <summary>
        /// The active player must discard before play can continue.
        /// </summary>
        Discard,

        /// <summary>
        /// The active player must choose a noble before play can continue.
        /// </summary>
        ChooseNoble,

        /// <summary>
        /// End of the game.
        /// </summary>
        End,
    }
}
